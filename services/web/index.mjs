import {
  ServiceDiscoveryClient,
  DiscoverInstancesCommand
} from '@aws-sdk/client-servicediscovery'

const client = new ServiceDiscoveryClient({
  region: 'eu-central-1'
})

function discoverInstances(serviceName = 'api') {
  const command = new DiscoverInstancesCommand({
    NamespaceName: 'app.com',
    ServiceName: serviceName
  })
  return client.send(command)
}

const { Instances } = await discoverInstances()

for (const instance of Instances) {
  console.log(instance)
}
