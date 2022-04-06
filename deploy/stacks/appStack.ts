import * as cdk from 'aws-cdk-lib'
import { aws_ec2, aws_servicediscovery, aws_ecs } from 'aws-cdk-lib'
import type { Construct } from 'constructs'

export class AppStack extends cdk.Stack {
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
        dnsRecordType: aws_servicediscovery.DnsRecordType.A,
        dnsTtl: cdk.Duration.minutes(1)
      }
    )

    const cluster = new aws_ecs.Cluster(this, 'cluster', {
      clusterName: 'app',
      vpc
    })

    const webSecurityGroup = new aws_ec2.SecurityGroup(
      this,
      'webSecurityGroup',
      {
        vpc,
        allowAllOutbound: true,
        description: 'Allow inbound HTTP'
      }
    )
    webSecurityGroup.addIngressRule(
      aws_ec2.Peer.anyIpv4(),
      aws_ec2.Port.tcp(80),
      'Allow HTTP'
    )

    const webService = new aws_ecs.CfnService(this, 'webService', {
      cluster: cluster.clusterName,
      desiredCount: vpc.isolatedSubnets.length,
      launchType: 'FARGATE',
      serviceName: 'web',
      taskDefinition: 'web:1',
      networkConfiguration: {
        awsvpcConfiguration: {
          securityGroups: [webSecurityGroup.securityGroupId],
          subnets: vpc.isolatedSubnets.map(subnet => subnet.subnetId),
          assignPublicIp: 'DISABLED'
        }
      }
    })

    const apiSecurityGroup = new aws_ec2.SecurityGroup(
      this,
      'apiSecurityGroup',
      {
        vpc,
        allowAllOutbound: true,
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
    endpoints
      .map(e => e.node.defaultChild as cdk.CfnResource)
      .forEach(child => apiService.addDependsOn(child))
    apiService.addDependsOn(
      apiDiscoverService.node.defaultChild as cdk.CfnResource
    )
  }
}

function registerEndpoints(
  scope: Construct,
  vpc: aws_ec2.Vpc
): aws_ec2.IGatewayVpcEndpoint[] {
  const endpointSecurityGroup = new aws_ec2.SecurityGroup(
    scope,
    'endpointSecurityGroup',
    {
      vpc,
      allowAllOutbound: true,
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
