#!/bin/sh

./update
version=`head -n 1 version.txt`
cat doxygen.conf | sed "s/%BUILD%/$version/" > doxygen.cf
doxygen doxygen.cf
