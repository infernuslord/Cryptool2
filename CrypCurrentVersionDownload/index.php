<html>
<head>
 <meta http-equiv="Content-Type" content="text/html;charset=utf-8">
 <meta http-equiv="refresh" content="1; URL=http://cryptool2.vs.uni-due.de/downloads/program/curversion/CrypTool-Setup-v2.0.3278a(beta).msi">
</head>
<body>
<?php
$config_file = 'CurrentVersion.txt';
$comment = "#";

if (file_exists($config_file)) {
	$fp = fopen($config_file, "r");

    while (!feof($fp)) {
      $line = trim(fgets($fp));
      if ($line && !ereg("^$comment", $line)) {
        $pieces = explode("=", $line);
        $option = trim($pieces[0]);
        $value = trim($pieces[1]);
        $config_values[$option] = $value;
      }
    }
    fclose($fp);
} else echo "No such file: " . $config_file;

?>
If your download doesn't start now, click <a href="<?php echo 'http://cryptool2.vs.uni-due.de/downloads/program/curversion/CrypTool-Setup-v' . $config_values['version'] . '.msi';?>">here</a>.

<script language="javascript" type="text/javascript">
<!-- // JavaScript-Bereich fr ltere Browser auskommentieren
    window.location.href = 'http://cryptool2.vs.uni-due.de/downloads/program/curversion/CrypTool-Setup-v<?php echo $config_values['version'];?>.msi';
// -->
</script>
</body>
</html>