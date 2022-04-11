import * as cdk from 'aws-cdk-lib'
import { aws_ec2, aws_ecs, aws_elasticloadbalancingv2 } from 'aws-cdk-lib'
import { Construct } from 'constructs'
import {
  LoadBalancedService,
  type LoadBalancedServiceProps
} from './loadBalancedService'

const endpointServiceIdMap = new Map<EndpointService, string>([
  [aws_ec2.GatewayVpcEndpointAwsService.S3, 'S3GatewayEndpoint'],
  [
    aws_ec2.InterfaceVpcEndpointAwsService.CLOUDWATCH_LOGS,
    'CloudwatchLogsEndpoint'
  ],
  [aws_ec2.InterfaceVpcEndpointAwsService.ECR, 'EcrApiEndpoint'],
  [aws_ec2.InterfaceVpcEndpointAwsService.ECR_DOCKER, 'EcrDockerEndpoint']
])

export type EndpointService =
  | aws_ec2.GatewayVpcEndpointAwsService
  | aws_ec2.InterfaceVpcEndpointAwsService

export interface AppVpcProps
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

type AddServicePropsSimple = Omit<
  aws_ecs.CfnServiceProps,
  'desiredCount' | 'launchType' | 'networkConfiguration' | 'loadBalancers'
>

interface AddServicePropsLoadBalanced extends AddServicePropsSimple {
  loadBalancedService: LoadBalancedService
  conditions: aws_elasticloadbalancingv2.ListenerCondition[]
  protectedAccess?: boolean
}

type AddServiceProps = AddServicePropsSimple &
  Partial<AddServicePropsLoadBalanced>

export class AppVpc extends aws_ec2.Vpc {
  private endpoints: aws_ec2.IVpcEndpoint[]

  constructor(scope: Construct, id: string, props: AppVpcProps) {
    super(scope, id, {
      ...props,
      cidr: '10.0.0.0/24',
      enableDnsHostnames: true,
      enableDnsSupport: true,
      maxAzs: 3,
      subnetConfiguration: [
        {
          name: 'Public',
          cidrMask: 27,
          subnetType: aws_ec2.SubnetType.PUBLIC
        },
        {
          name: 'Private',
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
            scope,
            'InterfaceEndpointServiceSecurityGroup',
            {
              vpc: this,
              description: 'Allow HTTPS from within the VPC'
            }
          )

          interfaceEndpointServiceSecurityGroup.addIngressRule(
            aws_ec2.Peer.ipv4(this.vpcCidrBlock),
            aws_ec2.Port.tcp(443),
            'Allow HTTPS from within the VPC'
          )
        }

        return new aws_ec2.InterfaceVpcEndpoint(
          scope,
          endpointServiceIdMap.get(service)!,
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
          scope,
          endpointServiceIdMap.get(service)!,
          {
            vpc: this,
            service,
            subnets: [{ subnetType: aws_ec2.SubnetType.PRIVATE_ISOLATED }]
          }
        )
      }
    })
  }

  addService(
    scope: Construct,
    id: string,
    props: AddServicePropsSimple
  ): aws_ecs.CfnService
  addService(
    scope: Construct,
    id: string,
    props: AddServicePropsLoadBalanced
  ): aws_ecs.CfnService
  addService(
    scope: Construct,
    id: string,
    props: AddServiceProps
  ): aws_ecs.CfnService {
    let securityGroup: aws_ec2.SecurityGroup
    let targetGroup:
      | aws_elasticloadbalancingv2.ApplicationTargetGroup
      | undefined
    let listenerRule:
      | cdk.aws_elasticloadbalancingv2.ApplicationListenerRule
      | undefined

    if (props.loadBalancedService) {
      props.protectedAccess ??= true

      securityGroup = props.loadBalancedService.securityGroup

      targetGroup = new aws_elasticloadbalancingv2.ApplicationTargetGroup(
        scope,
        id + 'TargetGroup',
        {
          vpc: this,
          targetType: aws_elasticloadbalancingv2.TargetType.IP,
          port: 80
        }
      )

      let listener: aws_elasticloadbalancingv2.ApplicationListener | undefined

      for (const _listener of props.loadBalancedService.loadBalancer
        .listeners) {
        if ((_listener as any).protocol === 'HTTPS') {
          listener = _listener
          break
        } else if ((_listener as any).protocol === 'HTTP') {
          listener = _listener
        }
      }

      if (!listener) {
        throw new Error('Load balancer must have a HTTP or HTTPS listener')
      }

      const conditions = [...props.conditions!]

      if (props.protectedAccess) {
        if (!props.loadBalancedService.loadBalancerARecord) {
          throw new Error(
            'For protected access the load balancer must have an associated A record'
          )
        }
        conditions.push(
          aws_elasticloadbalancingv2.ListenerCondition.hostHeaders([
            props.loadBalancedService.loadBalancerARecord.domainName,
            '*.' + props.loadBalancedService.loadBalancerARecord.domainName
          ])
        )
      }

      listenerRule = new aws_elasticloadbalancingv2.ApplicationListenerRule(
        scope,
        id + 'ListenerRule',
        {
          conditions,
          action: aws_elasticloadbalancingv2.ListenerAction.forward([
            targetGroup
          ]),
          listener,
          priority: 1
        }
      )
    } else {
      securityGroup = new aws_ec2.SecurityGroup(scope, id + 'SecurityGroup', {
        vpc: this,
        description: 'Allow traffic from within the vpc'
      })

      securityGroup.addIngressRule(
        aws_ec2.Peer.ipv4(this.vpcCidrBlock),
        aws_ec2.Port.tcp(80)
      )

      securityGroup.addIngressRule(
        aws_ec2.Peer.ipv4(this.vpcCidrBlock),
        aws_ec2.Port.tcp(443)
      )
    }

    const service = new aws_ecs.CfnService(scope, id, {
      ...props,
      desiredCount: 1,
      launchType: 'FARGATE',
      networkConfiguration: {
        awsvpcConfiguration: {
          securityGroups: [securityGroup.securityGroupId],
          subnets: this.isolatedSubnets.map(subnet => subnet.subnetId),
          assignPublicIp: 'DISABLED'
        }
      },
      ...(props.loadBalancedService
        ? {
            loadBalancers: [
              {
                containerName: props.serviceName,
                containerPort: 80,
                targetGroupArn: targetGroup!.targetGroupArn
              }
            ]
          }
        : {})
    })

    if (props.loadBalancedService) {
      service.addDependsOn(props.loadBalancedService)
      service.addDependsOn(targetGroup!.node.defaultChild as cdk.CfnResource)
      service.addDependsOn(listenerRule!.node.defaultChild as cdk.CfnResource)
    } else {
      service.addDependsOn(securityGroup.node.defaultChild as cdk.CfnResource)
    }

    this.endpoints.forEach(endpoint =>
      service.addDependsOn(endpoint.node.defaultChild as cdk.CfnResource)
    )

    return service
  }

  addLoadBalancedService(
    scope: Construct,
    id: string,
    props: LoadBalancedServiceProps
  ) {
    const service = new LoadBalancedService(scope, id, props)

    this.endpoints.forEach(endpoint =>
      service.addDependsOn(endpoint.node.defaultChild as cdk.CfnResource)
    )

    return service
  }
}

function isInterfaceEndpointService(
  endpointService: EndpointService
): endpointService is aws_ec2.InterfaceVpcEndpointAwsService {
  return endpointService instanceof aws_ec2.InterfaceVpcEndpointAwsService
}
