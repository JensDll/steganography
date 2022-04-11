import * as cdk from 'aws-cdk-lib'
import {
  aws_ecs,
  aws_elasticloadbalancingv2,
  aws_servicediscovery
} from 'aws-cdk-lib'
import { AppVpc } from '../services'

export interface ServicesStackProps extends cdk.StackProps {
  vpc: AppVpc
}

export class ServicesStack extends cdk.Stack {
  constructor(scope: cdk.App, id: string, props: ServicesStackProps) {
    super(scope, id, props)

    const cluster = new aws_ecs.Cluster(this, 'Cluster', {
      vpc: props.vpc,
      clusterName: 'App'
    })

    const loadBalancedWebService = props.vpc.addLoadBalancedService(
      this,
      'LoadBalancedWebService',
      {
        cluster: cluster.clusterName,
        serviceName: 'web',
        taskDefinition: 'web:7',
        loadBalancer: {
          vpc: props.vpc,
          aRecordDomainName: 'imagedatahiding.com',
          listenerCertificateArn:
            'arn:aws:acm:eu-central-1:378859530546:certificate/25d0d1fa-75c0-486f-afa8-8fcc65967d49'
        }
      }
    )

    // props.vpc.addService(this, 'ServiceDiscoveryService', {
    //   cluster: cluster.clusterName,
    //   serviceName: 'serviceDiscovery',
    //   taskDefinition: 'serviceDiscovery:4',
    //   loadBalancedService: loadBalancedWebService,
    //   conditions: [
    //     aws_elasticloadbalancingv2.ListenerCondition.pathPatterns([
    //       '/discover/*'
    //     ])
    //   ]
    // })

    // const appHttpNamespace = new aws_servicediscovery.HttpNamespace(
    //   this,
    //   'AppHttpNamespace',
    //   {
    //     name: 'app.com',
    //     description: 'The app service discovery namespace'
    //   }
    // )

    // const apiDiscoverService = appHttpNamespace.createService(
    //   'ApiDiscoverService',
    //   {
    //     name: 'api',
    //     description: 'Discovery service for the api'
    //   }
    // )

    // const apiService = props.vpc.addService(this, 'ApiService', {
    //   cluster: cluster.clusterName,
    //   serviceName: 'api',
    //   taskDefinition: 'api:5',
    //   serviceRegistries: [
    //     {
    //       registryArn: apiDiscoverService.serviceArn
    //     }
    //   ]
    // })

    // apiService.addDependsOn(
    //   apiDiscoverService.node.defaultChild as cdk.CfnResource
    // )
  }
}
