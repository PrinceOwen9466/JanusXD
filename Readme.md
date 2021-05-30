## JanusXD
![Logo](/JanusXD/wwwroot/images/logo.png)


Source Code to HTML\PDF Generation Tool



* Converts all files in your source code directory to a single or multiple HTML file(s)
* Ignores unnecessary files specified in `.gitignore` and `.janusignore` files 


**Help Instructions**

    janusxd --source /path/to/source/code --dest /output/folder
    --source, -s                            Source Directory
    --dest, -d                              Destination Folder
    --configure, -c                         Interactive Configuration
    --project, -p                           Set Project Name
    --disregard-git-ignore, -dg             Don't ignore paths included in .gitignore files
    --disregard-janus-ignore, -dj           Don't ignore paths included in .janusignore files
    --max-size, -m                          Maximum size (in bytes) of each html document
    --help, -h                              Help Instructions

  
**TODO**
* Title and Table of Contents Generation
* Automatic configuration to different [Code Prettify][0] styles
* Support for other syntax highlighting libraries
* Direct rendering and conversion to PDF

[0]: <https://github.com/googlearchive/code-prettify>