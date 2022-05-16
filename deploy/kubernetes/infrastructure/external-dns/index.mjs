import * as cdk from 'aws-cdk-lib'
import { aws_iam, aws_route53 } from 'aws-cdk-lib'

class ExternalDnsStack extends cdk.Stack {
  constructor(scope, id, props) {
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

    const userName = 'AllowDnsUpdates.ImageDataHiding'

    const user = new aws_iam.User(this, 'User', {
      userName
    })

    new aws_iam.Policy(this, userName, {
      document: policyDocument,
      policyName: userName,
      users: [user]
    })
  }
}

const app = new cdk.App()

new ExternalDnsStack(app, 'ExternalDns', {
  env: {
    region: 'eu-central-1',
    account: '378859530546'
  }
})

app.synth()
