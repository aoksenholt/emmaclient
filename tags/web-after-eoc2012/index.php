<?php

date_default_timezone_set("Europe/Stockholm");

include_once("./templates/emmalang_sv.php");
	include_once("./templates/classEmma.class.php");
   $lang = "sv";
   if (isset($_GET['lang']) && $_GET['lang'] != "")
   {
	$lang = $_GET['lang'];
   }
include_once("./templates/emmalang_$lang.php");


?>
<html>
<head><title><?=$_TITLE?></title>
<link rel="stylesheet" type="text/css" href="css/style.css">
<script language="javascript">
function colorRow(row)
{
	var el = document.getElementById(row);
	if (el == null)
	  return;
	el.style.backgroundColor = "#C0D6FF";
}
function resetRow(row)
{
var el = document.getElementById(row);
if (el == null)
  return;
el.style.backgroundColor = "";
}
</script>
</head>
<body topmargin="0" leftmargin="0">
<!-- MAIN DIV -->
<div class="maindiv">
<table width="759" cellpadding="0" cellspacing="0" border="0" ID="Table6">
	<tr>


		<TR>
			<TD>
		<a href="/liveresultat/"><img src="/pics/header_tjanster.jpg" alt="Svenska Orienteringsf�rbundet" align="left" width="759" height="91" border="0"></a>
			</TD>
		</TR>


</table>

<table border="0" cellpadding="0" cellspacing="0" width="759">
  <tr>
     <td valign="bottom">

<!-- MAIN MENU FLAPS - Two rows, note that left and right styles differs from middle ones -->
     <table border="0" cellpadding="0" cellspacing="0">
          <!-- Top row with rounded corners -->
          <tr>
               <td colspan="4"><span class="mttop"></td>
          </tr>
     </table>

     </td>
     <td align="right" valign="bottom">

     </td>
  </tr>
  <tr>
    <td class="submenu" colspan="2">
       <table border="0" cellpadding="0" cellspacing="0">
             <tr>
               <td><a href="index.php"><?=$_CHOOSECMP?></a></td>
             </tr>
       </table>
     </td>
  </tr>
<!-- End SUB MENU -->
  <tr>
    <td class="searchmenu" colspan="2" style="padding: 5px;">
       <table border="0" cellpadding="0" cellspacing="0" width="400">
             <tr>
               <td>
			| <?php echo($lang == "sv" ? "Svenska" : "<a href=\"?lang=sv\">Svenska</a>")?>
			| <?php echo($lang == "en" ? "English" : "<a href=\"?lang=en\">English</a>")?>
			| <?php echo($lang == "fi" ? "Suomeksi" : "<a href=\"?lang=fi\">Suomeksi</a>")?> |
			<h1 class="categoriesheader"><?=$_FIRSTPAGECHOOSE?></h1>
<!--<h1 class="categoriesheader"><?=$_FIRSTPAGELIVE?></h1>
<table border="0" cellpadding="0" cellspacing="0" width="100%">
			<tr id="row0"><td valign="top">2007-10-20</td><td>Elitserien final 2007, OK Enen<br/><a href="http://storatuna.mine.nu/emma/follow.php?comp=1230">Server #1</a></td></tr>
</table>
-->
						<h1 class="categoriesheader"><?=$_FIRSTPAGEARCHIVE?></h1>
			<table border="0" cellpadding="0" cellspacing="0" width="100%">
<?php
	$comps = Emma::GetCompetitions();
	//echo(sizeof($comps));
	//for ($i = 0; $i < sizeof($comps); $i++)
	foreach ($comps as $comp)
	{
	?>
		<tr id="row<?=$comp["tavid"]?>"><td><?=date("Y-m-d",strtotime($comp['compDate']))?></td><td><a onmouseover="colorRow('row<?=$comp["tavid"]?>')" onmouseout="resetRow('row<?=$comp["tavid"]?>')" href="templates/follow.php?comp=<?=$comp["tavid"]?>&lang=<?=$lang?>"><?=$comp["compName"]?></a></td><td><?=$comp["organizer"]?></td></tr>
	<?php
	}
	?>
			</table>
		</td>
	     </tr>
	</table>
<br><be>
Liveresultat p� n�tet �r en tj�nst utvecklad av Stora Tuna OK f�r att f�renkla f�r arrang�rer att publisera resultat fr�n orienteringst�vlingar p� n�tet.<br/>
Fr�n och med 2008 k�rs tj�nsten <i>p� prov</i> tillsammans med Svenska orienteringsf�rbundet p� www.obasen.nu<br/><br/>
Med ett enkelt program p� TC kan arrang�rerna erbjuda resultat som �r st�ndigt uppdaterad p� internet. Resultat som finns tillg�ngliga p� den h�r sidan skall inte ses som officiella utan f�r s�dana resultat h�nvisas till arrang�rens egen hemsida.<br>
Vill du som arrang�r ocks� erbjuda liveresultat p� n�tet fr�n er t�vling, kontakta <a href="mailto:peter@lofas.se">Peter L�f�s</a><br><br>
Vill du veta du det funkar? <a href="teknik.php">L�s mer om teknik</a><br><br>
     </td>
  </tr>

</table>
</div>
<br/><br/>
</body>
</html>