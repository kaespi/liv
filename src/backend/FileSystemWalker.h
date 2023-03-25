#pragma once

#include <QDir>
#include <QStringList>

class FileSystemWalker
{
  public:
    FileSystemWalker(const QString& file, QList<QByteArray>& fileTypes);

    QString getCurrentFile() const;
    QString getNextFile();
    QString getPrevFile();

  private:
    void updateFileIndexToExisting(int increment);
    void rescanFiles();

    QDir m_dir{};
    QStringList m_filesInDir{};
    int m_currentFileIndex{0};

    QStringList m_filePattern{"*.jpg", "*.JPG", "*.png", "*.PNG"};
};
