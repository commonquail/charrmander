<?php

$json = stripslashes($_POST['payload']);
$data = json_decode($json);

$downloadFile = 'VersionHistory.xml';
$url = 'nourl';
$author = 'noauthor';
$download = false;

if (isset($data) && isset($data->commits))
{
    foreach ($data->commits as $commit)
    {
        if (isset($commit->files))
        {
            foreach ($commit->files as $file)
            {
                if ($file->file === $downloadFile
                    && $file->type === "modified")
                {
                    $download = true;
                    $author = $commit->author;
                    $url = $data->canon_url . $data->repository->absolute_url
                        . 'raw/' . $commit->node . '/' . $file->file;
                }
            }
        }
    }
}
$s = date('Y-m-d') . ' ' . $author . ' ' . $uri
echo $s;

if ($download)
{
    file_put_contents($downloadFile, file_get_contents($url));
    file_put_contents('log', $s, FILE_APPEND);
}

?>