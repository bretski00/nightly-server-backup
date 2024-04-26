param (
    [string]$backupDestinationFolder,
    [string[]]$serverBackupItems
)

Write-Host "Backup Destination Folder: $backupDestinationFolder"
Write-Host "Server Backup Items: $($serverBackupItems -join ',')"

# Validate backup destination folder parameter
if (-not (Test-Path $backupDestinationFolder)) {
    Write-Host "Backup destination folder does not exist."
    Exit
}

# Get the current date and time
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

# Create the backup folder with timestamp
$backupDestinationFolder = Join-Path -Path $backupDestinationFolder -ChildPath $timestamp
New-Item -ItemType Directory -Path $backupDestinationFolder -ErrorAction SilentlyContinue

# Run the wbadmin backup command
$backupResult = Start-Process -FilePath wbadmin -ArgumentList "start backup -backupTarget:`"$backupDestinationFolder`" -include:$($serverBackupItems -join ',') -allCritical -quiet" -Wait -PassThru

# Check if the backup was successful
if ($backupResult.ExitCode -eq 0) {
    Write-Host "Backup completed successfully. Backup location: $backupDestinationFolder"
} else {
    Write-Host "Backup failed. Exit code: $($backupResult.ExitCode)"
}
