$fileName = "$PSScriptRoot/.aws.credentials"

function New-AwsCredentials {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    [string]$UserName,
    [switch]$Recreate
  )

  if (-not (Test-Path $fileName)) {
    Write-Verbose 'Creating new credentials files'
    New-Item $fileName -ItemType File 1> $null
  }

  if ($Recreate) {
    $accessKeys = (aws iam list-access-keys --user-name $UserName --query 'AccessKeyMetadata[].AccessKeyId' --output text) -split '\s+'

    Write-Verbose "Recreating AWS credentials for user '$UserName'"

    foreach ($accesKey in $accessKeys) {
      aws iam delete-access-key --access-key-id $accesKey --user-name $UserName
    }

    Write-AwsCredentials $UserName
  }
  else {
    if (Test-AwsCredentials $UserName) {
      throw "User '$UserName' already has cached credentials"
    }

    Write-Verbose "Creating new AWS credentials for user '$UserName'"

    Write-AwsCredentials $UserName
  }
}

function Read-AwsCredentials {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    [string]$UserName
  )

  if (-not (Test-AwsCredentials $UserName)) {
    throw "No crendentials found for user '$UserName'"
  }

  $accessKey = git config --file $fileName --get "$UserName.accessKey"
  $secretKey = git config --file $fileName --get "$UserName.secretKey"

  return @{
    AccessKey = $accessKey
    SecretKey = $secretKey
  }
}

function Remove-AwsCredentials {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    [string]$UserName
  )

  git config --file $fileName --remove-section $UserName
}

function Write-AwsCredentials {
  param(
    [Parameter(Mandatory)]
    [string]$UserName
  )

  $credentials = (aws iam create-access-key --user-name $UserName --query 'AccessKey.[AccessKeyId, SecretAccessKey]' --output text) -split '\s+'

  git config --file $fileName "$UserName.accessKey" $credentials[0]
  git config --file $fileName "$UserName.secretKey" $credentials[1]
}

function Test-AwsCredentials {
  param(
    [Parameter(Mandatory)]
    [string]$UserName
  )

  [bool]$result = git config --get --file $fileName "$UserName.accessKey"

  return $result
}

Export-ModuleMember -Function New-AwsCredentials, Read-AwsCredentials, Remove-AwsCredentials
