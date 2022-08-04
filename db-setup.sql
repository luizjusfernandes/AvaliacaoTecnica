CREATE TABLE userinfos (
    id INTEGER NOT NULL,
    username TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    PRIMARY KEY(id)
);

CREATE TABLE records (
    id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    code TEXT,
    name TEXT NOT NULL,
    cpf TEXT NOT NULL,
    address TEXT,
    phone TEXT,
    PRIMARY KEY(id),
    FOREIGN KEY(user_id) REFERENCES userinfos(id)
);