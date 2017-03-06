function Get-PublicKeyToken()
{
	[Cmdletbinding()]
     param(
		 [Parameter(Position=0, Mandatory=$true)]
		[string]$AssemblyPath
	 )
	$key = $null

	$bytes = $null
	$bytes = [System.Reflection.Assembly]::LoadFrom($AssemblyPath).GetName().GetPublicKeyToken()
	if ($bytes)
	{
		for($i=0; $i -lt $bytes.Length; $i++)
		{
			$key += "{0:x}" -f $bytes[$i]
		}
	}

	$key
 }