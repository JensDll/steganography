import 'source-map-support/register'
import * as cdk from 'aws-cdk-lib'
import { AppStack } from './stacks'

const app = new cdk.App()

new AppStack(app, 'App', {
  env: {
    account: process.env.CDK_DEFAULT_ACCOUNT,
    region: process.env.CDK_DEFAULT_REGION
  }
})

app.synth()
