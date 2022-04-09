import * as cdk from 'aws-cdk-lib'
import { aws_ec2, aws_ecs, aws_servicediscovery } from 'aws-cdk-lib'
import { AppNet } from '../services'

export class AppStack extends cdk.Stack {
  constructor(scope: cdk.App, id: string, props?: cdk.StackProps) {
    super(scope, id, props)

    const appNet = new AppNet(this, 'appNet', {
      endpoints: [
        aws_ec2.GatewayVpcEndpointAwsService.S3,
        aws_ec2.InterfaceVpcEndpointAwsService.CLOUDWATCH_LOGS,
        aws_ec2.InterfaceVpcEndpointAwsService.ECR,
        aws_ec2.InterfaceVpcEndpointAwsService.ECR_DOCKER
      ]
    })

    const cluster = new aws_ecs.Cluster(this, 'cluster', {
      vpc: appNet,
      clusterName: 'app'
    })

    appNet.addServiceWithLoadBalancer(
      'webService',
      {
        cluster: cluster.clusterName,
        serviceName: 'web',
        taskDefinition: 'web:1',
        containerName: 'web'
      },
      {
        aRecordDomainName: 'imagedatahiding.com',
        listenerCertificateArn:
          'arn:aws:acm:eu-central-1:378859530546:certificate/25d0d1fa-75c0-486f-afa8-8fcc65967d49'
      }
    )

    const appHttpNamespace = new aws_servicediscovery.HttpNamespace(
      this,
      'appHttpNamespace',
      {
        name: 'app.com',
        description: 'The app service discovery namespace'
      }
    )

    const apiDiscoverService = appHttpNamespace.createService(
      'apiDiscoverService',
      {
        name: 'api',
        description: 'Discovery service for the api'
      }
    )

    const apiService = appNet.addService('apiService', {
      cluster: cluster.clusterName,
      serviceName: 'api',
      taskDefinition: 'api:2',
      serviceRegistries: [
        {
          registryArn: apiDiscoverService.serviceArn
        }
      ]
    })

    apiService.addDependsOn(
      apiDiscoverService.node.defaultChild as cdk.CfnResource
    )
  }
}
