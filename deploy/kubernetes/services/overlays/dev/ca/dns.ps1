[CmdletBinding()]
param (
  [Parameter(Position = 0, Mandatory)]
  [ValidateSet('add', 'remove')]
  [string]$Action,

  [Parameter(Position = 1, Mandatory)]
  [string]$LoadBalancerIP
)

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')) {  
  Start-Process -FilePath 'pwsh' -Verb RunAs -ArgumentList $PSCommandPath, $Action, $LoadBalancerIP
  return
}

$hostsFile = 'C:\Windows\System32\drivers\etc\hosts'

switch ($Action) {
  add {
    Write-Verbose "Writing dev host entries to location: $hostsFile"
    
    $content = (Get-Content $hostsFile -Raw)
    $hasNewlime = $content -Match '\r\n$'

    @"
$($hasNewlime ? '' : "`r`n")$LoadBalancerIP dev.imagedatahiding.com
$LoadBalancerIP www.dev.imagedatahiding.com
$LoadBalancerIP api.dev.imagedatahiding.com
"@ | Add-Content -Path $hostsFile -NoNewline
  }
  remove {
    $lines = @()

    foreach ($line in Get-Content $hostsFile) {
      if ($line -NotMatch 'dev.imagedatahiding.com') {
        $lines += $line
      }
    }

    $lines | Out-File $hostsFile
  }
}