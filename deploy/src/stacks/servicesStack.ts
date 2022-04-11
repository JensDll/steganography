import * as cdk from 'aws-cdk-lib'
import { aws_ecs, aws_elasticloadbalancingv2 } from 'aws-cdk-lib'
import { AppVpc } from '../services'

export interface ServicesStackProps extends cdk.StackProps {
  vpc: AppVpc
}

export class ServicesStack extends cdk.Stack {
  constructor(scope: cdk.App, id: string, props: ServicesStackProps) {
    super(scope, id, props)

    const cluster = new aws_ecs.Cluster(this, 'Cluster', {
      vpc: props.vpc,
      clusterName: 'app-cluster'
    })

    const loadBalancedWebService = props.vpc.addLoadBalancedService(
      this,
      'LoadBalancedWebService',
      {
        cluster: cluster.clusterName,
        desiredCount: 1,
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

    props.vpc.addServiceBehindLoadBalancer(this, 'ApiService', {
      cluster: cluster.clusterName,
      desiredCount: 1,
      serviceName: 'api',
      taskDefinition: 'api:6',
      protocol: aws_elasticloadbalancingv2.ApplicationProtocol.HTTP,
      protocolVersion:
        aws_elasticloadbalancingv2.ApplicationProtocolVersion.HTTP1,
      loadBalancer: loadBalancedWebService,
      conditions: [
        aws_elasticloadbalancingv2.ListenerCondition.pathPatterns(['/api/*'])
      ]
    })
  }
}
