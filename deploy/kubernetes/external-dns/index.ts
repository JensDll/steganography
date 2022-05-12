import * as cdk from 'aws-cdk-lib'
import { aws_iam, aws_route53 } from 'aws-cdk-lib'

class MainStack extends cdk.Stack {
  constructor(scope: cdk.App, id: string, props?: cdk.StackProps) {
    super(scope, id, props)

    const domainName = 'imagehiding.com'

    const hostedZone = aws_route53.HostedZone.fromLookup(this, 'HostedZone', {
      domainName
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

    const userName = `AllowDnsUpdates@${domainName}`

    const dnsAccessUser = new aws_iam.User(this, 'DnsAccessUser', {
      userName
    })

    new aws_iam.Policy(this, userName, {
      document: policyDocument,
      policyName: userName,
      users: [dnsAccessUser]
    })
  }
}

const app = new cdk.App()

new MainStack(app, 'Main', {
  env: {
    region: 'eu-central-1',
    account: '378859530546'
  }
})

app.synth()
