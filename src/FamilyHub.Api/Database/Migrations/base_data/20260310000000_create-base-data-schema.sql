-- Base Data module: federal_states table
-- Stores German federal states (Bundeslaender) with ISO 3166-2 codes

CREATE SCHEMA IF NOT EXISTS base_data;

CREATE TABLE IF NOT EXISTS base_data.federal_states (
    id              uuid        NOT NULL PRIMARY KEY,
    name            varchar(100) NOT NULL,
    iso3166_code    varchar(6)  NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_federal_states_iso3166_code
    ON base_data.federal_states (iso3166_code);
