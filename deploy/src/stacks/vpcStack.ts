import * as cdk from 'aws-cdk-lib'
import { aws_ec2 } from 'aws-cdk-lib'
import { AppVpc } from '../services'

export class VpcStack extends cdk.Stack {
  vpc: AppVpc
  constructor(scope: cdk.App, id: string, props?: cdk.StackProps) {
    super(scope, id, props)

    this.vpc = new AppVpc(this, 'Vpc', {
      endpoints: [
        aws_ec2.GatewayVpcEndpointAwsService.S3,
        aws_ec2.InterfaceVpcEndpointAwsService.CLOUDWATCH_LOGS,
        aws_ec2.InterfaceVpcEndpointAwsService.ECR,
        aws_ec2.InterfaceVpcEndpointAwsService.ECR_DOCKER
      ]
    })
  }
}
