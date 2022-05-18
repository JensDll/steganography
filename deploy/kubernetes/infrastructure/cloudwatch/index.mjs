import * as cdk from 'aws-cdk-lib'
import { aws_iam } from 'aws-cdk-lib'

class CloudwatchStack extends cdk.Stack {
  constructor(scope, id, props) {
    super(scope, id, props)

    const user = new aws_iam.User(this, 'User', {
      userName: props.userName
    })

    user.addManagedPolicy(
      aws_iam.ManagedPolicy.fromManagedPolicyArn(
        this,
        'Policy',
        'arn:aws:iam::aws:policy/service-role/AmazonAPIGatewayPushToCloudWatchLogs'
      )
    )
  }
}

const app = new cdk.App()

const userName = process.env.AWS_USER_NAME

new CloudwatchStack(app, `Cloudwatch${userName}`, {
  userName: process.env.AWS_USER_NAME,
  env: {
    region: 'eu-central-1',
    account: '378859530546'
  }
})

app.synth()
