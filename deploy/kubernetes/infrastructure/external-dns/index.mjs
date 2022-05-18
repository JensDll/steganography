import * as cdk from 'aws-cdk-lib'
import { aws_iam, aws_route53 } from 'aws-cdk-lib'

class ExternalDnsStack extends cdk.Stack {
  constructor(scope, id, props) {
    super(scope, id, props)

    const hostedZone = aws_route53.HostedZone.fromLookup(this, 'HostedZone', {
      domainName: props.domainName
    })

    const policyDocument = new aws_iam.PolicyDocument({
      statements: [
        new aws_iam.PolicyStatement({
          effect: aws_iam.Effect.ALLOW,
          actions: ['route53:ChangeResourceRecordSets'],
          resources: [hostedZone.hostedZoneArn]
        }),
        new aws_iam.PolicyStatement({
          effect: aws_iam.Effect.ALLOW,
          actions: [
            'route53:ListHostedZones',
            'route53:ListResourceRecordSets'
          ],
          resources: ['*']
        })
      ]
    })

    const user = new aws_iam.User(this, 'User', {
      userName: props.userName
    })

    new aws_iam.Policy(this, 'Policy', {
      document: policyDocument,
      policyName: props.userName,
      users: [user]
    })
  }
}

const app = new cdk.App()

const userName = process.env.AWS_USER_NAME
const domainName = process.env.AWS_DOMAIN_NAME

new ExternalDnsStack(app, `ExternalDns${userName}`, {
  userName,
  domainName,
  env: {
    region: 'eu-central-1',
    account: '378859530546'
  }
})

app.synth()
