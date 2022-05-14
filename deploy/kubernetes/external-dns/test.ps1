
try {
  Push-Location $PSScriptRoot
  
  $json = cdk synth --json | ConvertFrom-Json -AsHashtable
  
  $user = $json.Resources.Values | Where-Object { $_.Type -eq 'AWS::IAM::User' }
  $userName = $user.Properties.UserName

  Write-Host "UserName: $userName"
}
finally {
  Pop-Location
}

