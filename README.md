# Submarine

## Tecnologias

- `.NET 8`) escolhido por ser um framework multiplataforma com amplo suporte
  para desenvolvimento de aplicações, tanto desktop quanto web, que utiliza
  C\#;
- `MSTest 3.0.4`) escolhido por ser um framework de teste bastante robusto e
  reconhecido muito bem integrado com as principais tecnologias de
  desenvolvimento com C\#, .NET, ASP.NET, Visual Studio.
- `dotnet-csharpier`, `csharp-ls`) usados para formatação e diagnóstico de
  código.

## Padrões

- `TDD`) escolhido para mapear as necessidades do programa rapidamente e se
  planejar de antemão quanto ao tempo de desenvolvimento, além de contribuir
  diretamente para cumprir a obrigatoriedade de testes unitários no projeto;
- `OOP`) escolhido para modelar a ideia do problema, modularizando o código e
  permitindo a expansão coesa da solução para outros problemas relacionados;
- `Conventional Commits`) escolhido para padronizar o formato das mensagens de
  commit.

## Detalhes de implementação

O dados de diagnósticos foram armazenados como um _array_ de _array_ de `byte`. Mas
a agregação dos bits para o cáculo das taxas gama e épsilon e as próprias taxas
usam valores `ulong`. Para a conversão de binário para decimal foi utilizada a
função `Convert.ToUInt64()` da biblioteca padrão. O cálculo do
consumo de energia foi separado em duas funções:
`DiagnosticReport.GetEnergyConsumption()` e
`DiagnosticReport.GetEnergyConsumptionBig()`. A primeria, faz o cálculo do
consumo com valores `ulong` e lança uma exceção caso ocorra um _overflow_, já
a segunda faz o cálculo utilizando `BigInteger` da biblioteca padrão. Em nenhum
outro caso é checado se há _overflow_, mas por não haver uma especificação
dos limites do tamanho da entrada do programa, essa alternativa foi incluída.

Para incluir um teste que exigisse o uso de `BigInteger`, um [arquivo de teste](https://github.com/vinicius-toshiyuki/Submarine/blob/main/SubmarineApp.Test/TestData/Dataset2.txt) foi feito com cerca de 1,5 milhão de linhas. A corretude do resultado para esse caso não foi checada, foi checada simplesmente a resiliência ao _overflow_ no cálculo, que produziu um resultado válido. Para incluir esse arquivo no repositório não foi utilizado `git lfs`, para evitar o passo extra de configuração do repositório para um projeto/exemplo tão simples.

Para ler os dados da entrada padrão foi utilizado um `StreamReader` da biblioteca padrão com a _stream_ do arquivo da entrada padrão para permitir ler toda a entrada de uma vez e não engasgar em casos com muitas linhas de entrada, como no exemplo do arquivo acima. Para testar o programa pelo console, foi utilizado o comando `cat <file path> | dotnet run --project SubmarineApp`.

O projeto foi desenvolvido e testado em ambiente Unix e não foi testado em ambiente Windows, apesar de ser compatível.
