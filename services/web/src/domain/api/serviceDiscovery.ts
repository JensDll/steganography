import {
  ServiceDiscoveryClient,
  DiscoverInstancesCommand
} from '@aws-sdk/client-servicediscovery'
import { CognitoIdentityClient } from '@aws-sdk/client-cognito-identity'
import { fromCognitoIdentityPool } from '@aws-sdk/credential-provider-cognito-identity'

export class Discovery {
  static IDENTITY_POOL_ID = 'eu-central-1:a7bb00e5-a9eb-41cf-8e48-e030d2acacd0'
  static ACCOUNT_ID = '378859530546'
  static REGION = 'eu-central-1'

  static Client = new ServiceDiscoveryClient({
    region: this.REGION,
    credentials: fromCognitoIdentityPool({
      client: new CognitoIdentityClient({ region: this.REGION }),
      identityPoolId: this.IDENTITY_POOL_ID
    })
  })

  static async discoverInstances(serviceName: 'api' = 'api') {
    const command = new DiscoverInstancesCommand({
      NamespaceName: 'app.com',
      ServiceName: serviceName
    })

    return this.Client.send(command)
  }
}
