USING_REGEX = /using\s*[A-Za-z0-9.-_]+;/
usings = []
parts = []
Dir.glob("TinyJSON/**/*.cs") do |path|
  next if path.include? "AssemblyInfo.cs"
  puts "Adding: #{path}"
  code = File.read path
  usings.concat code.scan(USING_REGEX)
  code.gsub! USING_REGEX, ""
  code.gsub! /namespace TinyJSON\s*{\s*\n/, ""
  code.gsub! /\u{FEFF}/, ""
  code.gsub! /^[\n\r]+/, "\n"
  code.gsub! /^}\s*\n/, ""
  parts << code
end

code = ""
code += usings.sort.uniq.join("\n") + "\n\n\n"
code += "namespace TinyJSON\n{"
code += parts.join("\n")
code += "}\n\n"

puts "Output: TinyJSON.cs"
File.open "TinyJSON.cs", "w" do |file|
  file.write code
end

