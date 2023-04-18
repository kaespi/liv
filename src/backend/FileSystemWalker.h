#pragma once

#include "IDir.h"
#include "IFileSystem.h"
#include "IFileSystemClassesFactory.h"
#include "IFileSystemWalker.h"

#include <QStringList>
#include <memory>

class FileSystemWalker : public IFileSystemWalker
{
  public:
    FileSystemWalker(const QString& file, const IFileSystemClassesFactory* ptrFsFactory,
                     QList<QByteArray>& fileTypes);
    virtual ~FileSystemWalker() = default;

    QString getCurrentFile() const override;
    QString getNextFile() override;
    QString getPrevFile() override;

  private:
    void updateFileIndexToExisting(int increment);
    void rescanFiles();

    std::unique_ptr<IDir> m_ptrDir{};
    std::unique_ptr<IFileSystem> m_ptrFileSystem{};
    QStringList m_filesInDir{};
    long long m_currentFileIndex{0};

    QStringList m_filePattern{"*.jpg", "*.JPG", "*.png", "*.PNG"};
};
