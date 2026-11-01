"""Apply the teaching extension to disposable, pinned source clones only."""
import argparse
from pathlib import Path
import subprocess
import shutil

parser = argparse.ArgumentParser()
parser.add_argument("sources", type=Path, help="Folder containing disposable Framework and Runner clones")
args = parser.parse_args()
root = args.sources.resolve()
expected = {
    "QaaS.Framework": "11f3ec66126cf878dc34c4fab8c152ced521f3ea",
    "QaaS.Runner": "7c3af33b093b500b0dc7fdcd2cffe2bb7f60c8ba",
}
for name, revision in expected.items():
    repo = root / name
    actual = subprocess.check_output(["git", "-C", str(repo), "rev-parse", "HEAD"], text=True).strip()
    dirty = subprocess.check_output(["git", "-C", str(repo), "status", "--porcelain"], text=True)
    if actual != revision or dirty:
        raise SystemExit(f"Use a clean disposable {name} clone at {revision}; no files changed.")

def replace(path, old, new):
    text = path.read_text(encoding="utf-8-sig")
    if text.count(old) != 1:
        raise RuntimeError(f"Expected one source anchor in {path}: {old!r}")
    path.write_text(text.replace(old, new), encoding="utf-8")

framework = root / "QaaS.Framework"
runner = root / "QaaS.Runner"
protocols = framework / "QaaS.Framework.Protocols"
shutil.copyfile(Path(__file__).with_name("JsonLinesSender.cs"), protocols / "JsonLinesSender.cs")
factory = protocols / "Protocols/Factories/SenderFactory.cs"
replace(factory, "// Singular senders", "// Singular senders\n           QaaS.Framework.Protocols.Examples.JsonLinesSenderConfig config => new QaaS.Framework.Protocols.Examples.JsonLinesSender(config),")

builders = runner / "QaaS.Runner.Sessions/Actions/Publishers/Builders"
properties = builders / "PublisherBuilderProperties.cs"
replace(properties, "public partial class PublisherBuilder : ICloneable<PublisherBuilder>\n{", "public partial class PublisherBuilder : ICloneable<PublisherBuilder>\n{\n    [Description(\"Appends compact JSON documents to a local file\")]\n    public QaaS.Framework.Protocols.Examples.JsonLinesSenderConfig? JsonLines { get; internal set; }\n")
logic = builders / "PublisherBuilderLogic.cs"
replace(logic, "RabbitMq = null;", "JsonLines = null;\n        RabbitMq = null;")
replace(logic, "case RabbitMqSenderConfig rabbitMqSenderConfig:", "case QaaS.Framework.Protocols.Examples.JsonLinesSenderConfig jsonLines:\n                JsonLines = jsonLines;\n                break;\n            case RabbitMqSenderConfig rabbitMqSenderConfig:")
replace(logic, "var allTypes = new List<ISenderConfig?>\n            {", "var allTypes = new List<ISenderConfig?>\n            {\n                JsonLines,")
replace(logic, "if (RabbitMq != null)\n            return RabbitMq;", "if (JsonLines != null)\n            return JsonLines;\n        if (RabbitMq != null)\n            return RabbitMq;")
chunk = runner / "QaaS.Runner.Sessions/Actions/ProtocolChunkSupport.cs"
replace(chunk, "[typeof(RabbitMqSenderConfig)]", "[typeof(QaaS.Framework.Protocols.Examples.JsonLinesSenderConfig)] = (ProtocolChunkMode.SingleOnly, \"JsonLines\"),\n            [typeof(RabbitMqSenderConfig)]")

# Only for this source-level laboratory. A real consumer uses coordinated NuGet releases.
infra = runner / "QaaS.Runner.Infrastructure/QaaS.Runner.Infrastructure.csproj"
replace(infra, '<PackageReference Include="QaaS.Framework.Executions" Version="1.7.1" />', '<ProjectReference Include="../../QaaS.Framework/QaaS.Framework.Executions/QaaS.Framework.Executions.csproj" />')
print("Applied sender, factory, Runner configuration/reset/selection/chunk wiring, and lab-only source reference.")
