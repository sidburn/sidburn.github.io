@echo off

call .paket\paket restore
call tools\fsformatting.exe literate --processDirectory --lineNumbers true --inputDirectory  "code" --outputDirectory "_posts" --waitforkey

