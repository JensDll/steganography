import * as cdk from 'aws-cdk-lib'
import {
  aws_ec2,
  aws_ecs,
  aws_servicediscovery,
  aws_elasticloadbalancingv2,
  aws_route53,
  aws_route53_targets
} from 'aws-cdk-lib'
import { Construct } from 'constructs'

export class MainStack extends cdk.Stack {
  constructor(scope: cdk.App, id: string, props?: cdk.StackProps) {
    super(scope, id, props)

    const vpc = new aws_ec2.Vpc(this, 'vpc', {
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

    const endpoints = registerEndpoints(this, vpc)

    const cluster = new aws_ecs.Cluster(this, 'cluster', {
      vpc,
      clusterName: 'app'
    })

    const { targetSecurityGroup, loadBalancerSecurityGroup } =
      createLoadBalancerSecurityGroupPair(this, vpc)

    const loadBalancer = new aws_elasticloadbalancingv2.ApplicationLoadBalancer(
      this,
      'loadBalancer',
      {
        vpc,
        internetFacing: true,
        securityGroup: loadBalancerSecurityGroup,
        vpcSubnets: { subnetType: aws_ec2.SubnetType.PUBLIC },
        deletionProtection: false
      }
    )

    const targetGroup = new aws_elasticloadbalancingv2.ApplicationTargetGroup(
      this,
      'targetGroup',
      {
        vpc,
        protocol: aws_elasticloadbalancingv2.ApplicationProtocol.HTTP,
        port: 80,
        targetType: aws_elasticloadbalancingv2.TargetType.IP
      }
    )

    const listener = new aws_elasticloadbalancingv2.ApplicationListener(
      this,
      'listener',
      {
        loadBalancer,
        protocol: aws_elasticloadbalancingv2.ApplicationProtocol.HTTP,
        port: 80,
        defaultAction: aws_elasticloadbalancingv2.ListenerAction.forward([
          targetGroup
        ])
      }
    )

    const zone = aws_route53.HostedZone.fromLookup(this, 'hostedZoneLookup', {
      domainName: 'imagedatahiding.com'
    })
    const target = aws_route53.RecordTarget.fromAlias(
      new aws_route53_targets.LoadBalancerTarget(loadBalancer)
    )
    new aws_route53.ARecord(this, 'loadBalancerARecord', {
      zone,
      target
    })

    const webService = new aws_ecs.CfnService(this, 'webService', {
      cluster: cluster.clusterName,
      desiredCount: vpc.isolatedSubnets.length,
      launchType: 'FARGATE',
      serviceName: 'web',
      taskDefinition: 'web:1',
      healthCheckGracePeriodSeconds: 60,
      loadBalancers: [
        {
          containerName: 'web',
          containerPort: 80,
          targetGroupArn: targetGroup.targetGroupArn
        }
      ],
      networkConfiguration: {
        awsvpcConfiguration: {
          securityGroups: [targetSecurityGroup.securityGroupId],
          subnets: vpc.isolatedSubnets.map(subnet => subnet.subnetId),
          assignPublicIp: 'DISABLED'
        }
      }
    })

    endpoints.forEach(endpoint =>
      webService.addDependsOn(endpoint.node.defaultChild as cdk.CfnResource)
    )
    webService.addDependsOn(targetGroup.node.defaultChild as cdk.CfnResource)
    webService.addDependsOn(listener.node.defaultChild as cdk.CfnResource)
    webService.addDependsOn(
      targetSecurityGroup.node.defaultChild as cdk.CfnResource
    )

    const appDnsNamespace = new aws_servicediscovery.PrivateDnsNamespace(
      this,
      'appDnsNamespace',
      {
        vpc,
        name: 'app.com',
        description: 'The app service discovery namespace'
      }
    )

    const apiDiscoverService = new aws_servicediscovery.Service(
      this,
      'apiDiscoverService',
      {
        namespace: appDnsNamespace,
        name: 'api',
        description: 'Discovery service for the web api',
        dnsRecordType: cdk.aws_servicediscovery.DnsRecordType.A,
        dnsTtl: cdk.Duration.minutes(1)
      }
    )

    const apiSecurityGroup = new aws_ec2.SecurityGroup(
      this,
      'apiSecurityGroup',
      {
        vpc,
        description: 'Allow everything'
      }
    )

    apiSecurityGroup.addIngressRule(
      aws_ec2.Peer.anyIpv4(),
      aws_ec2.Port.allTraffic(),
      'Allow everything'
    )

    const apiService = new aws_ecs.CfnService(this, 'apiService', {
      cluster: cluster.clusterName,
      desiredCount: vpc.isolatedSubnets.length,
      launchType: 'FARGATE',
      serviceName: 'api',
      taskDefinition: 'api:2',
      serviceRegistries: [
        {
          registryArn: apiDiscoverService.serviceArn
        }
      ],
      networkConfiguration: {
        awsvpcConfiguration: {
          securityGroups: [apiSecurityGroup.securityGroupId],
          subnets: vpc.isolatedSubnets.map(subnet => subnet.subnetId),
          assignPublicIp: 'DISABLED'
        }
      }
    })

    endpoints.forEach(endpoint =>
      apiService.addDependsOn(endpoint.node.defaultChild as cdk.CfnResource)
    )
    apiService.addDependsOn(
      apiDiscoverService.node.defaultChild as cdk.CfnResource
    )
    apiService.addDependsOn(
      apiSecurityGroup.node.defaultChild as cdk.CfnResource
    )
  }
}

function registerEndpoints(
  scope: Construct,
  vpc: aws_ec2.Vpc
): aws_ec2.IVpcEndpoint[] {
  const endpointSecurityGroup = new aws_ec2.SecurityGroup(
    scope,
    'endpointSecurityGroup',
    {
      vpc,
      description: 'Allow inbound HTTPS'
    }
  )

  endpointSecurityGroup.addIngressRule(
    aws_ec2.Peer.ipv4(vpc.vpcCidrBlock),
    aws_ec2.Port.tcp(443),
    'Allow HTTPS'
  )

  return [
    new aws_ec2.GatewayVpcEndpoint(scope, 's3GatewayEndpoint', {
      vpc,
      service: aws_ec2.GatewayVpcEndpointAwsService.S3,
      subnets: [{ subnetType: aws_ec2.SubnetType.PRIVATE_ISOLATED }]
    }),
    new aws_ec2.InterfaceVpcEndpoint(scope, 'cloudwatchLogsEndpoint', {
      vpc,
      service: aws_ec2.InterfaceVpcEndpointAwsService.CLOUDWATCH_LOGS,
      subnets: {
        subnets: vpc.isolatedSubnets,
        availabilityZones: vpc.availabilityZones
      },
      securityGroups: [endpointSecurityGroup]
    }),
    new aws_ec2.InterfaceVpcEndpoint(scope, 'ecrApiEndpoint', {
      vpc,
      service: aws_ec2.InterfaceVpcEndpointAwsService.ECR,
      subnets: {
        subnets: vpc.isolatedSubnets,
        availabilityZones: vpc.availabilityZones
      },
      securityGroups: [endpointSecurityGroup]
    }),
    new aws_ec2.InterfaceVpcEndpoint(scope, 'ecrDockerEndpoint', {
      vpc,
      service: aws_ec2.InterfaceVpcEndpointAwsService.ECR_DOCKER,
      subnets: {
        subnets: vpc.isolatedSubnets,
        availabilityZones: vpc.availabilityZones
      },
      securityGroups: [endpointSecurityGroup]
    })
  ]
}

function createLoadBalancerSecurityGroupPair(
  scope: Construct,
  vpc: cdk.aws_ec2.Vpc
) {
  const targetSecurityGroup = new aws_ec2.SecurityGroup(
    scope,
    'targetSecurityGroup',
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

  loadBalancerSecurityGroup.addIngressRule(
    aws_ec2.Peer.anyIpv4(),
    aws_ec2.Port.tcp(80),
    'Allow HTTP from anywhere'
  )

  loadBalancerSecurityGroup.addEgressRule(
    aws_ec2.Peer.securityGroupId(targetSecurityGroup.securityGroupId),
    aws_ec2.Port.tcp(80),
    'Allow HTTP to the target'
  )

  return {
    targetSecurityGroup,
    loadBalancerSecurityGroup
  }
}
