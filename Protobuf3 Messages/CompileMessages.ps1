$SourceDirectory="C:\Users\beals_000\Documents\MMOPPP\Protobuf3 Messages"
$DestinationDirectory="C:\Users\beals_000\Documents\MMOPPP\Protobuf3 Messages"
$DDCS = $DestinationDirectory+"\CSHARP"
$DDCP = $DestinationDirectory+"\CPP"

$ProtoFile = Read-Host 'Input proto filename (no .proto extension)'
$ProtoFilePath = $SourceDirectory+"\"+$ProtoFile+".proto"

protoc -I="$SourceDirectory" --csharp_out="$DDCS" "$ProtoFilePath"
protoc -I="$SourceDirectory" --cpp_out="$DDCP" "$ProtoFilePath"