USING_REGEX = /using\s*[A-Za-z0-9.-_]+;/
usings = []
parts = []
Dir.glob("TinyJSON/**/*.cs") do |path|
  next if path.include? "AssemblyInfo.cs"
  puts path
  code = File.read path
  # code = code.encode "ascii"
  usings.concat code.scan(USING_REGEX)
  code.gsub! USING_REGEX, ""
  code.gsub! /namespace TinyJSON\s*{\s*\n/, ""
  code.gsub! /\u{FEFF}/, ""
  code.gsub! /^[\n\r]+/, "\n"
  code.gsub! /^}\s*\n/, ""
  parts << code
  # puts code if path.include? "EncodeOptions.cs"
end

code = ""
code += usings.sort.uniq.join("\n") + "\n\n\n"
code += "namespace InControl.TinyJSON\n{"
code += parts.join("\n")
code += "}\n\n"

File.open "TinyJSON.cs", "w" do |file|
  file.write code
end