<?php
//Version: 1.0.1

//Display all errors
ini_set('display_errors', 1);
error_reporting(E_ALL);

//Update file info by cron
if ($_SERVER['REMOTE_ADDR'] === $_SERVER['SERVER_ADDR'] && isset($_GET['update'])) {
	//Download JSON
	$curl = curl_init();
	curl_setopt($curl, CURLOPT_URL, 'https://api.github.com/repos/bruhov/WinThumbsPreloader/releases');
	curl_setopt($curl, CURLOPT_RETURNTRANSFER, 1);
	curl_setopt($curl, CURLOPT_USERAGENT, 'WinThumbsPreloaderWebsite');
	$JSON = curl_exec($curl);
	curl_close($curl);
	//Parse
	$releases = json_decode($JSON);
	if ($releases === null || !isset($releases[0]->tag_name)) exit;
	$latestRelease = $releases[0];
	$setupFile = $latestRelease->assets[0];
	$fileInfo = [];
	$fileInfo['version'] = mb_substr($latestRelease->tag_name, 1);
	$fileInfo['URL'] = $setupFile->browser_download_url;
	$fileInfo['size'] = $setupFile->size;
	$fileInfo['downloadCount'] = 0;
	foreach ($releases as $release) $fileInfo['downloadCount'] += $release->assets[0]->download_count;
	//Save
	file_put_contents(__DIR__ . '/fileInfo.php', '<? $fileInfo = ' . var_export($fileInfo, true) . ';');
	exit;
}

//List of locales/language supported by the website (see "locale" folder)
$supportedLanguages = [
	'en_US.utf8' => 'en', //Default
	'ru_RU.utf8' => 'ru' 
];

$userLanguage = rtrim((string)($_GET['language'] ?? ''), '/');

//Unsupported language - return 404
if ($userLanguage !== '' && !in_array($userLanguage, $supportedLanguages)) {
	http_response_code(404);
	exit;
}

//If language is not provided, detect it automatically and redirect user
if ($userLanguage === '') {
	$userLanguage = (http\Env::negotiateLanguage($supportedLanguages) ?? $supportedLanguages['en_US.utf8']);
	header('Location: https://' . $_SERVER['HTTP_HOST'] . dirname($_SERVER['PHP_SELF']) . '/' . $userLanguage . '/');
	exit;
}

//Setup language
$userLocale = array_search($userLanguage, $supportedLanguages);
putenv('LC_MESSAGES=' . $userLocale);
setlocale(LC_MESSAGES, $userLocale);
bindtextdomain('messages', __DIR__ . '/locale');
bind_textdomain_codeset('messages', 'utf8');
textdomain('messages');

//Load file information
include(__DIR__ . '/fileInfo.php');

//Return page html
?><!doctype html>
<html lang="<?=$userLanguage?>">
	<head>
		<meta charset="utf-8">
		<meta http-equiv="x-ua-compatible" content="ie=edge">
		<title>WinThumbsPreloader - <?=_('Thumbnails preloader for Windows Explorer')?></title>
		<meta name="description" content="<?=_('A tool that automatically preloads all thumbnails for a directory and (optionally) subdirectories in Windows Explorer with just one click.')?>">
		<meta name="author" content="Dmitry Bruhov">
		<link rel="icon" href="../favicon.ico">
		<meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
		<link href="https://fonts.googleapis.com/css?family=Open+Sans:400,400i,700<?=$userLanguage === 'ru' ? '&amp;subset=cyrillic' : ''?>" rel="stylesheet">
		<link rel="stylesheet" href="../normalize.css">
		<link rel="stylesheet" href="../main.css">
	</head>
	<body>
		<div class="container">
			<div class="header">
				<h1>WinThumbsPreloader</h1>
				<p class="lead"><?=_('Thumbnails preloader for Windows Explorer')?></p>
				<div class="buttons">
					<div class="button download">
						<a class="downloadLink" href="<?=$fileInfo['URL']?>"><?=_('Download')?></a>
						<div class="downloadInfo">
							<b><?=_('Version')?>:</b> <?=$fileInfo['version']?> <a href="https://github.com/bruhov/WinThumbsPreloader/wiki/Changelog">(<?=_('changelog')?>)</a><br>
							<b><?=_('Download count')?>:</b> <?=(new \NumberFormatter($userLocale, \NumberFormatter::TYPE_INT32))->format($fileInfo['downloadCount'])?><br>
							<b><?=_('Size')?>:</b> <?=sprintf(_('%d KB'), ceil($fileInfo['size'] / 1024))?><br>
							<b><?=_('Platform')?>:</b> Windows 7/8/10
						</div>
						<div class="downloadInfoArrow"></div>
					</div>
					<a href="https://github.com/bruhov/WinThumbsPreloader/" class="button"><?=_('Source code')?></a>
				</div>
			</div>
			<div class="screenshotContainer">
				<video autoplay="autoplay" width="836" height="464" loop="loop" preload="auto">
					<source src="../images/preview.mp4" type='video/mp4'>
				</video>
			</div>
			<p class="description"><?=_('<b>WinThumbsPreloader</b> is a simple open source tool for preloading thumbnails in Windows Explorer. Just right click on the folder to call the context menu and select <i>WinThumbsPreloader&nbsp;>&nbsp;Preload thumbnails</i> in the menu.')?></p>
			<div class="featuresContainer">
				<h2><?=_('Key features')?></h2>
				<ul>
					<li><?=sprintf(_('Free, no advertisements, %sopen source%s'), '<a href="https://github.com/bruhov/WinThumbsPreloader/">', '</a>')?></li>
					<li><?=_('Preload thumbnails for entire folder and (optionally) it\'s subfolders')?></li>
					<li><?=_('Integration with Windows Explorer')?></li>
					<li><?=_('Command line interface')?></li>
				</ul>
			</div>
			<div class="footer">
				<div>
					<div class="copyright">Copyright © 2018 <a href="https://github.com/bruhov">Dmitry Bruhov</a></div>
					<div class="license"><?=sprintf(_('Distributed under %sThe MIT License%s'), '<a href="https://github.com/bruhov/WinThumbsPreloader/blob/master/LICENSE" target="_blank">', '</a>')?></div>
				</div>
				<?php
					$languages = [
						'en' => 'English',
						'ru' => 'Русский'
					];
				?>
				<div class="language">
					<div class="languages">
						<? foreach ($languages as $languageCode => $languageName): ?>
							<a class="languageLink" href="../<?=$languageCode?>/"><span class="flag <?=$languageCode?>"></span> <?=$languageName?></a>
						<? endforeach ?>
					</div>
					<?=_('Site language')?>: <span class="flag <?=$userLanguage?>"></span><b><?=$languages[$userLanguage]?></b>
				</div>
			</div>
		</div>
	</body>
</html>