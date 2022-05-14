$fileName = '.aws.credentials'

function Get-AwsCredentials {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    [string]$UserName,
    [switch]$Recreate
  )

  if (-not (Test-Path $fileName)) {
    New-Item -Path $PSScriptRoot -Name $fileName -ItemType File 1> $null
  }

  $doesNotHaveCredentials = -not (git config --get --file $fileName "$UserName.accessKey")

  if ($Recreate) {
    $accessKeys = (aws iam list-access-keys --user-name $UserName --query 'AccessKeyMetadata[].AccessKeyId' --output text) -split '\s+'

    Write-Verbose "Recreating AWS credentials for user '$UserName'"

    foreach ($accesKey in $accessKeys) {
      aws iam delete-access-key --access-key-id $accesKey --user-name $UserName
    }

    New-AwsCredentials $UserName
  } elseif ($doesNotHaveCredentials) {
    Write-Verbose "Creating new AWS credentials for user '$UserName'"

    New-AwsCredentials $UserName
  } else {
    Write-Verbose 'Using existing AWS credentials'
  }

  $accessKey = git config --file $fileName --get "$UserName.accessKey"
  $secretKey = git config --file $fileName --get "$UserName.secretKey"

  return @{
    AccessKey = $accessKey
    SecretKey = $secretKey
  }
}

function New-AwsCredentials {
  param(
    [Parameter(Mandatory)]
    [string]$UserName
  )

  $credentials = (aws iam create-access-key --user-name $UserName --query 'AccessKey.[AccessKeyId, SecretAccessKey]' --output text) -split '\s+'

  git config --file $fileName "$UserName.accessKey" $credentials[0]
  git config --file $fileName "$UserName.secretKey" $credentials[1]
}

Export-ModuleMember -Function Get-AwsCredentials
