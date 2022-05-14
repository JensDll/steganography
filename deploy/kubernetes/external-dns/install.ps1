<#
.PARAMETER Recreate
    Recreate AWS credentials.
#>

[CmdletBinding()]
param (
  [Alias('r')]
  [switch]$Recreate,
  [Parameter(ValueFromRemainingArguments)]
  [string[]]$HelmInstallArgs
)

function Write-AwsCredentials {
  param(
    [Parameter(Mandatory)]
    [string]$UserName
  )

  aws iam create-access-key --user-name $UserName --query 'AccessKey.[AccessKeyId, SecretAccessKey]' --output text |
    Out-File -FilePath "$PSScriptRoot/aws.credentials" -NoNewline
}

function Read-AwsCredentials {
  return (Get-Content -Path "$PSScriptRoot/aws.credentials" -Raw) -split '\s+'
}

try {
  Push-Location $PSScriptRoot
  
  $json = cdk synth --json | ConvertFrom-Json -AsHashtable
  
  $user = $json.Resources.Values | Where-Object { $_.Type -eq 'AWS::IAM::User' }
  $userName = $user.Properties.UserName

  cdk deploy
} finally {
  Pop-Location
}

$keyIds = (aws iam list-access-keys --user-name $userName --query 'AccessKeyMetadata[].AccessKeyId' --output text) -split '\s+'

if ($keyIds -and $Recreate) {
  Write-Verbose "Recreating AWS credentials for user '$userName'"

  foreach ($keyId in $keyIds) {
    aws iam delete-access-key --access-key-id $keyId --user-name $userName
  }

  Write-AwsCredentials $userName
  
} elseif (-not (Test-Path "$PSScriptRoot/aws.credentials")) {
  Write-Verbose "Creating new AWS credentials for user '$UserName'"
  Write-AwsCredentials $userName
} else {
  Write-Verbose 'Using existing AWS credentials'
}

$credentials = Read-AwsCredentials

helm upgrade external-dns bitnami/external-dns --install --namespace=external-dns `
  --create-namespace `
  --set "aws.credentials.accessKey=$credentials[0],aws.credentials.secretKey=$credentials[1]" `
  --values="$PSScriptRoot/values.yaml" $HelmInstallArgs
