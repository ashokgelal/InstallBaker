#!/bin/bash

update_git_hooks() 
{
  echo "UPDATING GIT HOOKS"
  for file in tools/githooks/*; do 
    base=$(basename $file)
    githook=.git/hooks/$base
    diff $file $githook >/dev/null 2>&1 
    if (( $? )); then
      echo "new $base detected, updating"
      cp $file $githook >/dev/null 2>&1
    else
      echo "$base unmodified"
    fi
    if [ ! -x "$githook" ]; then
      echo "making $githook executable"
      chmod +x $githook
    fi
  done

}

update_git_aliases() 
{
  echo "UPDATING GIT ALIASES"
  git config --unset alias.commit
  git config alias.co "checkout"
  git config alias.sf "submodule foreach"
  git config alias.siu "!git submodule update --init --recursive"
  git config alias.pullup "!sh -c 'git pull \$1 \$2 && git siu' -"
  git config alias.checkup "!sh -c 'git checkout \$1 && git siu' -"
  git config alias.fetchup "!sh -c 'git fetch \$1 --prune && git sf git fetch \$1 --prune' -"
  git config alias.send "!tools/pre-push.sh && git push"
  git config alias.new "!sh -c 'git init && git remote add origin \$1 && git config branch.master.remote origin && git config branch.master.merge refs/heads/master && git config remote.origin.push refs/heads/master:refs/heads/master && git config remote.origin.pull refs/heads/master:refs/heads/master' -"
  git config alias.rh "reset --hard"
  git config alias.k "!gitk --all --select-commit=HEAD &"
  git config alias.cam "commit -am"
  git config alias.cup "commit --amend -am"
  git config alias.mp "!sh -c 'git checkout master && git pull' -"

  git config color.ui "auto"
  git config color.diff "auto"
  git config color.status "auto"
  git config color.branch "auto"
}

# MAIN

echo "*** BIG BROTHER ***"
update_git_hooks
update_git_aliases

exit 0
