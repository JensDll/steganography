import 'source-map-support/register'
import * as cdk from 'aws-cdk-lib'
import {
  VpcStack,
  ServicesStack,
  IdentityPoolStack,
  TaskDefinitionStack
} from './src/stacks'

const app = new cdk.App()

const vpcStack = new VpcStack(app, 'Vpc', {
  env: {
    region: process.env.CDK_DEFAULT_REGION,
    account: process.env.CDK_DEFAULT_ACCOUNT
  }
})

new ServicesStack(app, 'Services', {
  env: {
    region: process.env.CDK_DEFAULT_REGION,
    account: process.env.CDK_DEFAULT_ACCOUNT
  },
  vpc: vpcStack.vpc
})

new IdentityPoolStack(app, 'IdentityPool', {
  env: {
    region: process.env.CDK_DEFAULT_REGION,
    account: process.env.CDK_DEFAULT_ACCOUNT
  }
})

new TaskDefinitionStack(app, 'TaskDefinitions', {
  env: {
    region: process.env.CDK_DEFAULT_REGION,
    account: process.env.CDK_DEFAULT_ACCOUNT
  }
})

app.synth()
