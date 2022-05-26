[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [String]
    $AssemblyInfoFilePath,
    [Parameter(Mandatory=$true)]
    [String]
    $BuildNumber,
    [Parameter(Mandatory=$false)]
    [String]
    $PatchVersion,
    [Parameter(Mandatory=$false)]
    [String]
    $MinorVersion,
    [Parameter(Mandatory=$false)]
    [String]
    $MajorVersion
)

Write-Host "Updating $AssemblyInfoFilePath"

$updatedContent = Get-Content $AssemblyInfoFilePath |
Foreach-Object {
    if ($_ -match '^\[assembly: Assembly(File)?Version')
    {
        $_ -match '(?<MajorVersion>\d+).(?<MinorVersion>\d+).(?<PatchVersion>\d+)(.(?<BuildNumber>\d+))?' | Out-Null

        $newMajorVersion = $matches.MajorVersion
        $newMinorVersion = $matches.MinorVersion
        $newPatchVersion = $matches.PatchVersion
        $newBuildNumber = $matches.BuildNumber

        if ($PSBoundParameters.ContainsKey('MajorVersion'))
        {
            Write-Host "Changing major version from $newMajorVersion to $MajorVersion"
            $newMajorVersion = $MajorVersion
        }
        
        if ($PSBoundParameters.ContainsKey('MinorVersion'))
        {
            Write-Host "Changing minor version from $newMinorVersion to $MinorVersion"
            $newMinorVersion = $MinorVersion
        }
        
        if ($PSBoundParameters.ContainsKey('PatchVersion'))
        {
            Write-Host "Changing patch version from $newPatchVersion to $PatchVersion"
            $newPatchVersion = $PatchVersion
        }
        
        Write-Host "Changing build number from $newBuildNumber to $BuildNumber"
        $newBuildNumber = $BuildNumber

        $updatedLine = $_ -replace '((\d+)\.?)+', "$newMajorVersion.$newMinorVersion.$newPatchVersion.$newBuildNumber"
        Write-Host "Changing $_ to $updatedLine"
        $updatedLine
    }
    else
    {
        $_
    }
}

$versionString = "$newMajorVersion.$newMinorVersion.$newPatchVersion.$newBuildNumber"

Write-Output "::set-output name=CURRENT_DATE::$(Get-Date -Format 'MM/dd/yyyy')"
Write-Output "::set-output name=RELEASE_VERSION::$($versionString)"

Set-Content -Path $AssemblyInfoFilePath -Value $updatedContent