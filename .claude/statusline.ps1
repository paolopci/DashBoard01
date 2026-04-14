$inputJson = [Console]::In.ReadToEnd()

try {
    $data = $inputJson | ConvertFrom-Json
    $usedPercentage = $data.context_window.used_percentage

    if ($null -eq $usedPercentage) {
        Write-Output "CONTEXT: --%"
        exit 0
    }

    $roundedPercentage = [Math]::Round([double]$usedPercentage)
    Write-Output "CONTEXT: $roundedPercentage%"
}
catch {
    Write-Output "CONTEXT: --%"
}
