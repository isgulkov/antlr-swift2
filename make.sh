java -jar antlr-4.7-complete.jar Swift.g4 -o src
xbuild src/SwiftTranslator.sln
mkdir -p ./res
cp src/bin/Debug/* ./res
