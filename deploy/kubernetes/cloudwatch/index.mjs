import * as cdk from 'aws-cdk-lib'
import { aws_iam } from 'aws-cdk-lib'

class CloudwatchStack extends cdk.Stack {
  constructor(scope, id, props) {
    super(scope, id, props)

    const userName = `CloudwatchAccess@image-data-hiding`

    const cloudwatchAccessUser = new aws_iam.User(
      this,
      'CloudwatchAccessUser',
      {
        userName
      }
    )

    cloudwatchAccessUser.addManagedPolicy(
      aws_iam.ManagedPolicy.fromManagedPolicyArn(
        this,
        'Policy',
        'arn:aws:iam::aws:policy/service-role/AmazonAPIGatewayPushToCloudWatchLogs'
      )
    )
  }
}

const app = new cdk.App()

new CloudwatchStack(app, 'Cloudwatch', {
  env: {
    region: 'eu-central-1',
    account: '378859530546'
  }
})

app.synth()
