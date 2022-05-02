import 'source-map-support/register'
import * as cdk from 'aws-cdk-lib'
import { VpcStack, ServicesStack, TaskDefinitionStack } from './src/stacks'

const app = new cdk.App()

const vpcStack = new VpcStack(app, 'Net', {
  env: {
    region: process.env.CDK_DEFAULT_REGION,
    account: process.env.CDK_DEFAULT_ACCOUNT
  }
})

const taskDefinitionStack = new TaskDefinitionStack(app, 'TaskDefinitions', {
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
  vpc: vpcStack.vpc,
  taskDefinitions: {
    web: taskDefinitionStack.web,
    api: taskDefinitionStack.api
  }
})

app.synth()
