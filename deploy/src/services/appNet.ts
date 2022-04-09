import * as cdk from 'aws-cdk-lib'
import {
  aws_certificatemanager,
  aws_ec2,
  aws_ecs,
  aws_elasticloadbalancingv2,
  aws_route53,
  aws_route53_targets
} from 'aws-cdk-lib'
import { Construct } from 'constructs'

export type EndpointService =
  | aws_ec2.GatewayVpcEndpointAwsService
  | aws_ec2.InterfaceVpcEndpointAwsService

export interface AppNetProps
  extends Omit<
    aws_ec2.VpcProps,
    | 'cidr'
    | 'enableDnsHostnames'
    | 'enableDnsSupport'
    | 'maxAzs'
    | 'subnetConfiguration'
  > {
  endpoints?: EndpointService[]
}

export type AddServiceProps = Omit<
  aws_ecs.CfnServiceProps,
  'desiredCount' | 'launchType' | 'networkConfiguration' | 'loadBalancers'
>

export type LoadBalancerProps = {
  aRecordDomainName: string
  listenerCertificateArn: string
}

const serviceIdLookup = new Map<EndpointService, string>([
  [aws_ec2.GatewayVpcEndpointAwsService.S3, '_s3GatewayEndpoint'],
  [
    aws_ec2.InterfaceVpcEndpointAwsService.CLOUDWATCH_LOGS,
    '_cloudwatchLogsEndpoint'
  ],
  [aws_ec2.InterfaceVpcEndpointAwsService.ECR, '_ecrApiEndpoint'],
  [aws_ec2.InterfaceVpcEndpointAwsService.ECR_DOCKER, '_ecrDockerEndpoint']
])

export class AppNet extends aws_ec2.Vpc {
  private endpoints: aws_ec2.IVpcEndpoint[]

  constructor(scope: Construct, id: string, props: AppNetProps) {
    super(scope, id, {
      ...props,
      cidr: '10.0.0.0/24',
      enableDnsHostnames: true,
      enableDnsSupport: true,
      maxAzs: 3,
      subnetConfiguration: [
        {
          name: 'public',
          cidrMask: 27,
          subnetType: aws_ec2.SubnetType.PUBLIC
        },
        {
          name: 'private',
          cidrMask: 27,
          subnetType: aws_ec2.SubnetType.PRIVATE_ISOLATED
        }
      ]
    })

    props.endpoints ??= []

    let interfaceEndpointServiceSecurityGroup: aws_ec2.SecurityGroup

    this.endpoints = props.endpoints.map(service => {
      if (isInterfaceEndpointService(service)) {
        if (!interfaceEndpointServiceSecurityGroup) {
          interfaceEndpointServiceSecurityGroup = new aws_ec2.SecurityGroup(
            this,
            'interfaceEndpointServiceSecurityGroup',
            {
              vpc: this,
              description: 'Allow inbound HTTPS'
            }
          )

          interfaceEndpointServiceSecurityGroup.addIngressRule(
            aws_ec2.Peer.ipv4(this.vpcCidrBlock),
            aws_ec2.Port.tcp(443),
            'Allow HTTPS'
          )
        }

        return new aws_ec2.InterfaceVpcEndpoint(
          this,
          serviceIdLookup.get(service)!,
          {
            vpc: this,
            service,
            subnets: {
              subnets: this.isolatedSubnets,
              availabilityZones: this.availabilityZones
            },
            securityGroups: [interfaceEndpointServiceSecurityGroup]
          }
        )
      } else {
        return new aws_ec2.GatewayVpcEndpoint(
          this,
          serviceIdLookup.get(service)!,
          {
            vpc: this,
            service,
            subnets: [{ subnetType: aws_ec2.SubnetType.PRIVATE_ISOLATED }]
          }
        )
      }
    })
  }

  addService(id: string, props: AddServiceProps) {
    const serviceSecurityGroup = new aws_ec2.SecurityGroup(
      this,
      id + '_securityGroup',
      {
        vpc: this,
        description: 'Allow traffic from within the vpc'
      }
    )

    serviceSecurityGroup.addIngressRule(
      aws_ec2.Peer.ipv4(this.vpcCidrBlock),
      aws_ec2.Port.tcp(80)
    )

    serviceSecurityGroup.addIngressRule(
      aws_ec2.Peer.ipv4(this.vpcCidrBlock),
      aws_ec2.Port.tcp(443)
    )

    const serviceProps: aws_ecs.CfnServiceProps = {
      ...props,
      desiredCount: this.isolatedSubnets.length,
      launchType: 'FARGATE',
      networkConfiguration: {
        awsvpcConfiguration: {
          securityGroups: [serviceSecurityGroup.securityGroupId],
          subnets: this.isolatedSubnets.map(subnet => subnet.subnetId),
          assignPublicIp: 'DISABLED'
        }
      }
    }

    const service = new aws_ecs.CfnService(this, id, serviceProps)

    this.endpoints.forEach(endpoint =>
      service.addDependsOn(endpoint.node.defaultChild as cdk.CfnResource)
    )

    return service
  }

  addServiceWithLoadBalancer(
    id: string,
    props: AddServiceProps & { containerName: string },
    loadBalancerProps: LoadBalancerProps
  ) {
    const { targetSecurityGroup, loadBalancerSecurityGroup } =
      createSecurityGroupPair(this, this, id)

    const loadBalancer = new aws_elasticloadbalancingv2.ApplicationLoadBalancer(
      this,
      id + '_loadBalancer',
      {
        vpc: this,
        internetFacing: true,
        vpcSubnets: { subnetType: aws_ec2.SubnetType.PUBLIC },
        deletionProtection: false,
        securityGroup: loadBalancerSecurityGroup
      }
    )

    const targetGroup = new aws_elasticloadbalancingv2.ApplicationTargetGroup(
      this,
      id + '_targetGroup',
      {
        vpc: this,
        targetType: aws_elasticloadbalancingv2.TargetType.IP,
        port: 80
      }
    )

    const httpListener = new aws_elasticloadbalancingv2.ApplicationListener(
      this,
      id + '_httpListener',
      {
        loadBalancer,
        port: 80,
        defaultAction: aws_elasticloadbalancingv2.ListenerAction.redirect({
          protocol: aws_elasticloadbalancingv2.ApplicationProtocol.HTTPS,
          permanent: true,
          port: '443'
        })
      }
    )

    const httpsListener = new aws_elasticloadbalancingv2.ApplicationListener(
      this,
      id + '_httpsListener',
      {
        loadBalancer,
        port: 443,
        defaultAction: aws_elasticloadbalancingv2.ListenerAction.forward([
          targetGroup
        ]),
        certificates: [
          aws_elasticloadbalancingv2.ListenerCertificate.fromArn(
            loadBalancerProps.listenerCertificateArn
          )
        ]
      }
    )

    new aws_route53.ARecord(this, 'loadBalancerARecord', {
      zone: aws_route53.HostedZone.fromLookup(this, 'hostedZoneLookup', {
        domainName: loadBalancerProps.aRecordDomainName
      }),
      target: aws_route53.RecordTarget.fromAlias(
        new aws_route53_targets.LoadBalancerTarget(loadBalancer)
      )
    })

    const serviceProps: aws_ecs.CfnServiceProps = {
      ...props,
      desiredCount: this.isolatedSubnets.length,
      launchType: 'FARGATE',
      networkConfiguration: {
        awsvpcConfiguration: {
          securityGroups: [targetSecurityGroup.securityGroupId],
          subnets: this.isolatedSubnets.map(subnet => subnet.subnetId),
          assignPublicIp: 'DISABLED'
        }
      },
      loadBalancers: [
        {
          containerName: props.containerName,
          containerPort: 80,
          targetGroupArn: targetGroup.targetGroupArn
        }
      ]
    }

    const service = new aws_ecs.CfnService(this, id, serviceProps)

    service.addDependsOn(targetGroup.node.defaultChild as cdk.CfnResource)
    service.addDependsOn(httpListener.node.defaultChild as cdk.CfnResource)
    service.addDependsOn(httpsListener.node.defaultChild as cdk.CfnResource)
    service.addDependsOn(
      targetSecurityGroup.node.defaultChild as cdk.CfnResource
    )
    this.endpoints.forEach(endpoint =>
      service.addDependsOn(endpoint.node.defaultChild as cdk.CfnResource)
    )

    return { service, loadBalancer }
  }
}

function isInterfaceEndpointService(
  endpointService: EndpointService
): endpointService is aws_ec2.InterfaceVpcEndpointAwsService {
  return endpointService instanceof aws_ec2.InterfaceVpcEndpointAwsService
}

function createSecurityGroupPair(
  scope: Construct,
  vpc: aws_ec2.IVpc,
  targetId: string
) {
  const targetSecurityGroup = new aws_ec2.SecurityGroup(
    scope,
    targetId + 'SecurityGroup',
    {
      vpc
    }
  )

  const loadBalancerSecurityGroup = new aws_ec2.SecurityGroup(
    scope,
    'loadBalancerSecurityGroup',
    {
      vpc,
      allowAllOutbound: false,
      // Avoid circular dependency by using the AWS::EC2::SecurityGroupEgress
      // and AWS::EC2::SecurityGroupIngress resources.
      // https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/aws-properties-ec2-security-group
      disableInlineRules: true
    }
  )

  targetSecurityGroup.addIngressRule(
    aws_ec2.Peer.securityGroupId(loadBalancerSecurityGroup.securityGroupId),
    aws_ec2.Port.tcp(80),
    'Allow HTTP from the load balancer'
  )

  targetSecurityGroup.addIngressRule(
    aws_ec2.Peer.securityGroupId(loadBalancerSecurityGroup.securityGroupId),
    aws_ec2.Port.tcp(443),
    'Allow HTTPS from the load balancer'
  )

  loadBalancerSecurityGroup.addIngressRule(
    aws_ec2.Peer.anyIpv4(),
    aws_ec2.Port.tcp(80),
    'Allow HTTP from anywhere'
  )

  loadBalancerSecurityGroup.addIngressRule(
    aws_ec2.Peer.anyIpv4(),
    aws_ec2.Port.tcp(443),
    'Allow HTTPS from anywhere'
  )

  loadBalancerSecurityGroup.addEgressRule(
    aws_ec2.Peer.securityGroupId(targetSecurityGroup.securityGroupId),
    aws_ec2.Port.tcp(80),
    'Allow HTTP to the target'
  )

  loadBalancerSecurityGroup.addEgressRule(
    aws_ec2.Peer.securityGroupId(targetSecurityGroup.securityGroupId),
    aws_ec2.Port.tcp(443),
    'Allow HTTPS to the target'
  )

  return {
    targetSecurityGroup,
    loadBalancerSecurityGroup
  }
}
