[CmdletBinding()]
param (
    [String]
    $CsprojFileName,
    [String]
    $CsprojFilePath = "."
)

# $xml.Save will only work with an absolute path
# therefore resolving the path of the CSPROJ file
$csprojAbsoluteFilePath = Join-Path $CsprojFilePath $CsprojFileName | Resolve-Path
Write-Host "Updating $csprojAbsoluteFilePath"

$xml = [xml](Get-Content $csprojAbsoluteFilePath)

$references = $xml.Project.ItemGroup |
    Foreach-Object { $_.Reference } |
    Where-Object { $_.Include -match "VMS.TPS.Common.Model" }

$references |
Foreach-Object {
    # Removing everything after the package name (e.g. Version, Culture, PublicKeyToken, etc.)
    $newInclude = $_.Include -replace ",.*", ""
    Write-Host "Replacing $($_.Include) with $newInclude"

    $_.RemoveAll()
    $_.SetAttribute("Include", $newInclude)

    $specificVersion = $xml.CreateElement("SpecificVersion", $xml.DocumentElement.NamespaceURI)
    $specificVersion.InnerText = "False"
    $_.AppendChild($specificVersion) | Out-Null

    $copyToLocal = $xml.CreateElement("Private", $xml.DocumentElement.NamespaceURI)
    $copyToLocal.InnerText = "False"
    $_.AppendChild($copyToLocal) | Out-Null

    $hintPath = $xml.CreateElement("HintPath", $xml.DocumentElement.NamespaceURI)
    $hintPath.InnerText = Join-Path $Env:GITHUB_WORKSPACE_PACKAGES_PATH "$newInclude.dll"
    $_.AppendChild($hintPath) | Out-Null
    Write-Host "Added new hint path for ${newInclude}: $($hintPath.InnerText)"
}

Write-Host "Saving updated CSPROJ"
$xml.Save($csprojAbsoluteFilePath)