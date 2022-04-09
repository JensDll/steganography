import * as cdk from 'aws-cdk-lib'
import { aws_cognito, aws_iam } from 'aws-cdk-lib'

export class IdentityPoolStack extends cdk.Stack {
  constructor(scope: cdk.App, id: string, props?: cdk.StackProps) {
    super(scope, id, props)

    const identityPool = new aws_cognito.CfnIdentityPool(this, 'identityPool', {
      identityPoolName: 'app',
      allowUnauthenticatedIdentities: true
    })

    const assumedBy = new aws_iam.WebIdentityPrincipal(
      'cognito-identity.amazonaws.com'
    )

    const cognitoAuthenticatedRole = new aws_iam.Role(
      this,
      'cognitoAuthenticatedRole',
      {
        assumedBy,
        inlinePolicies: {
          cognito_default: new aws_iam.PolicyDocument({
            statements: [
              new aws_iam.PolicyStatement({
                effect: aws_iam.Effect.ALLOW,
                actions: ['mobileanalytics:PutEvents', 'cognito-sync:*'],
                resources: ['*']
              })
            ]
          })
        }
      }
    )

    const cognitoUnauthenticatedRole = new aws_iam.Role(
      this,
      'cognitoUnauthenticatedRole',
      {
        assumedBy,
        inlinePolicies: {
          cognito_default: new aws_iam.PolicyDocument({
            statements: [
              new aws_iam.PolicyStatement({
                effect: aws_iam.Effect.ALLOW,
                actions: ['mobileanalytics:PutEvents', 'cognito-sync:*'],
                resources: ['*']
              })
            ]
          })
        },
        managedPolicies: [
          aws_iam.ManagedPolicy.fromAwsManagedPolicyName(
            'AWSCloudMapReadOnlyAccess'
          )
        ]
      }
    )

    new aws_cognito.CfnIdentityPoolRoleAttachment(
      this,
      'identityPoolRoleAttachment',
      {
        identityPoolId: identityPool.ref,
        roles: {
          authenticated: cognitoAuthenticatedRole.roleArn,
          unauthenticated: cognitoUnauthenticatedRole.roleArn
        }
      }
    )
  }
}
