[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [String]
    $EclipseVersion,
    [Parameter(Mandatory=$true)]
    [String]
    $ExpirationDate,
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

$versionMapping = @{
    "18.0" = @{
        Version = "18.0"
        EsapiVersion = "18.0.0.1"
        NetFrameworkVersion = "48"
    };
    "17.0" = @{
        Version = "17.0"
        EsapiVersion = "17.0.0"
        NetFrameworkVersion = "48"
    };
    "16.1" = @{
        Version = "16.1"
        EsapiVersion = "16.1.0"
        NetFrameworkVersion = "461"
    };
    "15.6" = @{
        Version = "15.6"
        EsapiVersion = "15.6.0"
        NetFrameworkVersion = "45"
    };
}

$versionConfig = $versionMapping[$EclipseVersion]

if (-not $versionConfig) {
    throw "Invalid Eclipse version $EclipseVersion provided"
}

$esapiPackagesPath = "..\packages\ESAPI.$($versionConfig.EsapiVersion)\lib\net$($versionConfig.NetFrameworkVersion)\"

$configurationFileFullPath = Resolve-Path "Configuration.props"
$esapiReferenceFileFullPath = Resolve-Path "EsapiReference.props"

Get-ChildItem -Recurse "AssemblyInfo.cs" |
ForEach-Object {
    Write-Host "Updating AssemblyExpirationDate in $($_.FullName)"
    (Get-Content $_) -replace "#{AssemblyExpirationDate}#",$ExpirationDate |
    Set-Content $_ 
}

$configurationProps = [xml](Get-Content $configurationFileFullPath)
$currentVersions = $configurationProps.Project.PropertyGroup |
Where-Object { $_.AssemblyVersion }

$currentVersions.AssemblyVersion -match '(?<MajorVersion>\d+).(?<MinorVersion>\d+).(?<PatchVersion>\d+).(?<BuildNumber>\d+)?' |
Out-Null

$newMajorVersion = $matches.MajorVersion
$newMinorVersion = $matches.MinorVersion
$newPatchVersion = $matches.PatchVersion
$newBuildNumber = $matches.BuildNumber

if ($PSBoundParameters.ContainsKey('MajorVersion'))
{
    Write-Host "Changing major version from $newMajorVersion to $MajorVersion"
    $newMajorVersion = $MajorVersion
} else {
    Write-Host "Major version is $newMajorVersion"
}

if ($PSBoundParameters.ContainsKey('MinorVersion'))
{
    Write-Host "Changing minor version from $newMinorVersion to $MinorVersion"
    $newMinorVersion = $MinorVersion
} else {
    Write-Host "Minor version is $newMinorVersion"
}

if ($PSBoundParameters.ContainsKey('PatchVersion'))
{
    Write-Host "Changing patch version from $newPatchVersion to $PatchVersion"
    $newPatchVersion = $PatchVersion
} else {
    Write-Host "Patch version is $newPatchVersion"
}

Write-Host "Changing build number from $newBuildNumber to $BuildNumber"
$newBuildNumber = $BuildNumber

$versionString = "$newMajorVersion.$newMinorVersion.$newPatchVersion.$newBuildNumber"
Write-Host "Changing version numbers from $($currentVersions.AssemblyVersion) to $versionString"

$currentVersions.AssemblyVersion = $versionString
$currentVersions.FileVersion = $versionString
$currentVersions.InformationalVersion = $versionString

Write-Host "Saving updated Configuration.props"
$configurationProps.Save($configurationFileFullPath)


$releaseName = "$ProjectName-V$versionString-$(Get-Date -Format 'MM/dd/yyyy')($ExpirationDate)"
$normalizedReleaseName = "$ProjectName-V$versionString-$(Get-Date -Format 'MM-dd-yyyy').$($ExpirationDate -replace '/','-')"

$esapiReferenceProps = [xml](Get-Content $esapiReferenceFileFullPath)

$references = $esapiReferenceProps.Project.ItemGroup |
    Foreach-Object { $_.Reference } |
    Where-Object { $_.Include -match "VMS.TPS.Common.Model" }

$references |
Foreach-Object {
    # Removing everything after the package name (e.g. Version, Culture, PublicKeyToken, etc.)
    $newInclude = $_.Include -replace ",.*", ""
    $_.RemoveAll()
    $_.SetAttribute("Include", $newInclude)

    $specificVersion = $esapiReferenceProps.CreateElement("SpecificVersion", $esapiReferenceProps.DocumentElement.NamespaceURI)
    $specificVersion.InnerText = "False"
    $_.AppendChild($specificVersion) | Out-Null

    $copyToLocal = $esapiReferenceProps.CreateElement("Private", $esapiReferenceProps.DocumentElement.NamespaceURI)
    $copyToLocal.InnerText = "False"
    $_.AppendChild($copyToLocal) | Out-Null

    $hintPath = $esapiReferenceProps.CreateElement("HintPath", $esapiReferenceProps.DocumentElement.NamespaceURI)
    $hintPath.InnerText = Join-Path $esapiPackagesPath "$newInclude.dll"
    $_.AppendChild($hintPath) | Out-Null
    Write-Host "Added new hint path for ${newInclude}: $($hintPath.InnerText)"
}

Write-Host "Saving updated ESAPI hint paths"
$esapiReferenceProps.Save($esapiReferenceFileFullPath)

"ESAPI_VERSION=$($versionConfig.EsapiVersion)" >> $env:GITHUB_OUTPUT
"RELEASE_VERSION=$($versionString)" >> $env:GITHUB_OUTPUT
"RELEASE_NAME=$($releaseName)" >> $env:GITHUB_OUTPUT
"RELEASE_FILE_NAME=$($normalizedReleaseName)" >> $env:GITHUB_OUTPUT