#
# Script.ps1
#

$UserCredential = Get-Credential
$Session = New-PSSession -ConfigurationName Microsoft.Exchange -ConnectionUri https://outlook.office365.com/powershell-liveid/ -Credential $UserCredential -Authentication Basic -AllowRedirection
Import-PSSession $Session -DisableNameChecking -AllowClobber


get-aduser -Filter {Department -eq 'KARSEN Karrieresenter Østfold' -and SamAccountName -ne 'username'} | % {    
    "$($_.SamAccountName):\kalender"
    Remove-MailboxFolderPermission -Identity "$($_.SamAccountName):\kalender" -User username -Confirm $false
}


get-aduser -Filter {Department -eq 'KARSEN Karrieresenter Østfold' -and SamAccountName -ne 'username'} | % {
	 "$($_.SamAccountName):\kalender"
    Add-MailboxFolderPermission -Identity "$($_.SamAccountName):\kalender" -User username -AccessRights Editor -SharingPermissionFlags Delegate
}

