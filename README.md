# SaintsBuild #

## Development ##

1.  `git clone --branch unity ${your-folk-git-url} SaintsBuild`
2.  `git rm Assets/SaintsBuild`
3.  `rm -rf .git/modules/SaintsBuild`
4.  `git config --remove-section submodule.Assets/SaintsBuild`
5.  `git submodule add --force ${your-folk} Assets/SaintsBuild`
6.  `cd Assets/SaintsBuild` and checkout to your editing branch, e.g. `git checkout master`
7.  windows: `.\link.cmd`

    mac/linux: `ln -s 'Assets/SaintsBuild/Samples~' 'Assets/SaintsBuild/Samples'`
