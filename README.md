# Tracker™ 

Bittorrent tracker written in C# that serves the [UDP tracker protocol](http://www.bittorrent.org/beps/bep_0015.html)

Uses redis as a memcache for peer and seeder info, so it needs a redis instance running on localhost to work

## Todo

- [ ] keep track of transaction id's (tracker naively accepts any id at the moment)
- [ ] some sort of UI or config to manage the tracker while its running
- [ ] think of a name
