# GoatSim Offline Beta

Experimental Python CLI. Not a Windows executable. No DPS accuracy verified.

Run: `python tools/goatsim_beta.py --export examples/goatsim-synthetic-evidence.json --priority examples/goatsim-synthetic-priority.json --duration-ms 3000`

Fixtures are synthetic. Requires Python 3.11+. No game input, injection or network access.

Run tests: `python -m unittest discover -s tests -p "test_goatsim*.py"`.

Import requires local `goatsim-export-v1` JSON with explicitly typed self direct hits.
Supply a separate ordered JSON priority list with skill names and cooldown_ms.
The program does not fetch shugo.gg or derive cooldowns, skill coefficients, or stats.
It estimates damage from observed median per-hit values, not actual cast mechanics.
It excludes other players, unknown damage kinds, and DoTs.
Default delay is 10ms; optional 20ms. Action lock is a caller assumption.
The 17.1K individual Templar row and 25.5K preliminary log estimate are unresolved.
Never commit real logs containing private character or packet data.
