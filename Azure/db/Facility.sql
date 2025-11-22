CREATE SCHEMA poc;

CREATE TABLE poc.facility
(
    facility_id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,   -- Maps to Dataverse primary key (GUID)
    client_id UNIQUEIDENTIFIER NOT NULL,                -- FK to the owning Account, Dataverse Account GUID
    name NVARCHAR(200) NOT NULL,                        -- Primary display name
    facility_type NVARCHAR(100) NULL,                   -- Optional: e.g., Office, Warehouse, Store
    address NVARCHAR(300) NULL,                         -- Optional address
    created_on DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    modified_on DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- Index on client_id for quick lookups
CREATE INDEX ix_facility_client_id ON poc.facility(client_id);

/*sample data, replace client_id GUIDs with actual Account GUIDs from your Dataverse environment*/
INSERT INTO poc.facility (facility_id, client_id, name, facility_type, address)
VALUES
('2bf4f201-0000-0000-0000-000000000001', '47e56ab6-b7c7-f011-8544-6045bd9613e8', 'Sheffield Warehouse', 'Warehouse', '21 Clay Lane, Sheffield'),
('2bf4f202-0000-0000-0000-000000000002', '47e56ab6-b7c7-f011-8544-6045bd9613e8', 'Manchester Distribution Hub', 'Distribution Centre', '10 Mill Street, Manchester'),
('2bf4f203-0000-0000-0000-000000000003', '82835bc8-bbc7-f011-8544-6045bd9613e8', 'Brightwater Store #14', 'Retail Store', '55 High Street, Leeds'),
('2bf4f204-0000-0000-0000-000000000004', '82835bc8-bbc7-f011-8544-6045bd9613e8', 'Brightwater HQ', 'Office', '8 Kingsway, London');
