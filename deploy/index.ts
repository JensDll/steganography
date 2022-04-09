import 'source-map-support/register'
import * as cdk from 'aws-cdk-lib'
import { AppStack, IdentityPoolStack } from './src/stacks'

const app = new cdk.App()

new AppStack(app, 'App', {
  env: {
    region: process.env.CDK_DEFAULT_REGION,
    account: process.env.CDK_DEFAULT_ACCOUNT
  }
})

new IdentityPoolStack(app, 'IdentityPool', {
  env: {
    region: process.env.CDK_DEFAULT_REGION,
    account: process.env.CDK_DEFAULT_ACCOUNT
  }
})

app.synth()
