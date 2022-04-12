import * as cdk from 'aws-cdk-lib'
import { aws_ecs, aws_ecr, aws_iam, aws_logs } from 'aws-cdk-lib'

export class TaskDefinitionStack extends cdk.Stack {
  constructor(scope: cdk.App, id: string, props?: cdk.StackProps) {
    super(scope, id, props)

    const repository = aws_ecr.Repository.fromRepositoryName(
      this,
      'Repository',
      'image-data-hiding'
    )
    const executionRole = aws_iam.Role.fromRoleName(
      this,
      'ExecutionRole',
      'ecsTaskExecutionRole'
    )

    const webTaskDefinition = new aws_ecs.TaskDefinition(this, 'web', {
      family: 'web',
      cpu: '256',
      memoryMiB: '512',
      networkMode: aws_ecs.NetworkMode.AWS_VPC,
      compatibility: aws_ecs.Compatibility.FARGATE,
      executionRole,
      taskRole: executionRole,
      runtimePlatform: {
        cpuArchitecture: aws_ecs.CpuArchitecture.ARM64,
        operatingSystemFamily: aws_ecs.OperatingSystemFamily.LINUX
      }
    })

    const webContainerDefinition = webTaskDefinition.addContainer('web', {
      essential: true,
      image: aws_ecs.ContainerImage.fromEcrRepository(repository, 'web.latest'),
      portMappings: [
        {
          containerPort: 443,
          hostPort: 443,
          protocol: aws_ecs.Protocol.TCP
        }
      ],
      logging: aws_ecs.LogDriver.awsLogs({
        logGroup: new aws_logs.LogGroup(this, 'LogGroup', {
          logGroupName: 'app/web'
        }),
        streamPrefix: 'ecs'
      })
    })
    webContainerDefinition.logDriverConfig!.options!['awslogs-create-group'] =
      'true'

    const apiTaskDefinition = new aws_ecs.TaskDefinition(this, 'api', {
      family: 'api',
      cpu: '2048',
      memoryMiB: '4096',
      networkMode: aws_ecs.NetworkMode.AWS_VPC,
      compatibility: aws_ecs.Compatibility.FARGATE,
      executionRole,
      taskRole: executionRole,
      runtimePlatform: {
        cpuArchitecture: aws_ecs.CpuArchitecture.ARM64,
        operatingSystemFamily: aws_ecs.OperatingSystemFamily.LINUX
      }
    })

    apiTaskDefinition.addContainer('api', {
      essential: true,
      image: aws_ecs.ContainerImage.fromEcrRepository(repository, 'api.latest'),
      portMappings: [
        {
          containerPort: 80,
          hostPort: 80,
          protocol: aws_ecs.Protocol.TCP
        },
        {
          containerPort: 443,
          hostPort: 443,
          protocol: aws_ecs.Protocol.TCP
        }
      ]
    })
  }
}
