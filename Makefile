# Roboya Dünyası — root task runner. Keep CLAUDE.md §4 in sync with this file.
SHELL := /bin/bash
.DEFAULT_GOAL := help

DOTNET ?= $(shell command -v dotnet 2>/dev/null || echo /usr/local/share/dotnet/dotnet)
UNITY_VERSION ?= $(shell sed -n 's/^m_EditorVersion: //p' apps/game/ProjectSettings/ProjectVersion.txt 2>/dev/null)
UNITY ?= /Applications/Unity/Hub/Editor/$(UNITY_VERSION)/Unity.app/Contents/MacOS/Unity
ENGINE_SLN := tools/engine-dotnet/Roboya.Engine.slnx

.PHONY: help setup api-dev web-dev editor-dev test lint gen validate-content fix-content unity-test \
        test-api test-web test-engine test-content lint-api lint-web check-gen

help: ## List commands
	@grep -E '^[a-zA-Z_-]+:.*?## ' $(MAKEFILE_LIST) | awk 'BEGIN{FS=":.*?## "}{printf "  \033[36m%-18s\033[0m %s\n",$$1,$$2}'

setup: ## Install deps, start Docker services, load seed data
	@test -f .env || cp .env.example .env
	git lfs install --local
	cd apps/api && uv sync
	npm install
	$(DOTNET) restore $(ENGINE_SLN)
	docker compose -f infra/docker-compose.yml up -d --wait
	cd apps/api && uv run alembic upgrade head && uv run python -m app.seed

api-dev: ## Run the API locally with reload
	cd apps/api && uv run uvicorn app.main:app --reload --port 8000

web-dev: ## Run the web panels locally
	npm run dev -w @roboya/web

editor-dev: ## Run the internal level editor locally
	npm run dev -w @roboya/level-editor

test: test-engine test-api test-web test-content ## Run all API, web, engine and content tests

test-engine:
	$(DOTNET) test $(ENGINE_SLN) --nologo -v q /p:CollectCoverage=true /p:Threshold=90 /p:ThresholdType=line /p:Include="[Roboya.CodingEngine]*" /p:ExcludeByFile="**/Generated/*.cs"

test-api:
	cd apps/api && uv run pytest

test-web:
	npm run test --workspaces --if-present

test-content: validate-content

lint: lint-api lint-web ## ruff, mypy, eslint, type checks

lint-api:
	cd apps/api && uv run ruff check . && uv run ruff format --check . && uv run mypy

lint-web:
	npm run lint --workspaces --if-present
	npm run typecheck --workspaces --if-present

gen: ## Generate C# / TypeScript code from the level schema and OpenAPI
	npm run gen -w @roboya/level-schema
	@if [ -d packages/api-contract/src ]; then \
		cd apps/api && uv run python -m app.export_openapi ../../packages/api-contract/openapi.json && cd ../.. && \
		npm run gen -w @roboya/api-contract; \
	fi

check-gen: gen ## Fail if generated code is out of date (used in CI)
	git diff --exit-code -- packages apps/game/Assets/_Project/Scripts/CodingEngine/Levels/Generated

validate-content: ## Validate all levels (schema + solver) and the voice manifest
	npm run validate -w @roboya/level-schema
	$(DOTNET) run --project tools/engine-dotnet/Roboya.LevelValidator -c Release -- content/levels --voice content/voice/script.csv

fix-content: ## Write solver-computed shortest lengths into level files
	$(DOTNET) run --project tools/engine-dotnet/Roboya.LevelValidator -c Release -- content/levels --voice content/voice/script.csv --fix

unity-test: ## Run Unity EditMode tests in batch mode
	"$(UNITY)" -batchmode -nographics -projectPath apps/game -runTests -testPlatform EditMode \
		-testResults "$(CURDIR)/apps/game/TestResults/editmode.xml" -logFile -
