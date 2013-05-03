#!/bin/sh

echo "Checking all required packages..."

if [ ! -f `which xbuild` ];then
	echo "mono-xbuild is not installed!"
	exit 1
fi

echo "Checking links"

if [ ! -L manualpages ];then
	ln -s ManualPages manualpages
fi

xbuild || exit 1

if [ ! -f pidgeon ];then
	echo "#!/bin/sh" >> pidgeon
	echo "mono bin/Debug/Pidgeon.exe $*" >> pidgeon
	chmod a+x pidgeon
fi

if [ -f buildall.sh ];then
	sh buildall.sh
fi

